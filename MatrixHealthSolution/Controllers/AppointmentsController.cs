using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using MatrixHealthSolution.Models.ViewModels;
using MatrixHealthSolution.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers;

public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AppointmentSlotService _slots;
    private readonly EmailService _email;
    private readonly IConfiguration _config;

    public AppointmentsController(ApplicationDbContext context, AppointmentSlotService slots, EmailService email, IConfiguration config)
    {
        _context = context;
        _slots = slots;
        _email = email;
        _config = config;
    }

    // GET: /Appointments/Book
    [HttpGet]
    public async Task<IActionResult> Book(DateTime? date = null, int? serviceId = null)
    {
        var chosenDate = (date ?? DateTime.Today).Date;

        var vm = new BookAppointmentVM
        {
            Date = chosenDate,
            ServiceId = serviceId ?? 0,
            DepositAmount = 25m
        };

        vm.Services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();

        // Don’t show times until a service is selected
        if (vm.ServiceId > 0)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == vm.ServiceId && s.IsActive);

            if (service != null)
                vm.AvailableTimes = await _slots.GetAvailableSlotsAsync(chosenDate, service.DurationMinutes);
            else
                vm.AvailableTimes = new List<TimeSpan>();
        }
        else
        {
            vm.AvailableTimes = new List<TimeSpan>();
        }

        return View(vm); // Views/Appointments/Book.cshtml
    }

    // POST: /Appointments/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookAppointmentVM vm)
    {
        vm.Services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == vm.ServiceId && s.IsActive);

        if (service == null)
        {
            vm.AvailableTimes = new List<TimeSpan>();
            ModelState.AddModelError("", "Please choose a valid service.");
            return View(vm);
        }

        // Load available times using overrides + duration + overlap rules
        vm.AvailableTimes = await _slots.GetAvailableSlotsAsync(vm.Date.Date, service.DurationMinutes);

        if (!ModelState.IsValid)
            return View(vm);

        if (!vm.AvailableTimes.Contains(vm.StartTime))
        {
            ModelState.AddModelError("", "That time slot is no longer available. Please choose another.");
            return View(vm);
        }

        var scheduledAt = vm.Date.Date.Add(vm.StartTime);

        var appt = new Appointment
        {
            ServiceId = vm.ServiceId,
            ScheduledAt = scheduledAt,
            DurationMinutes = service.DurationMinutes,

            // patient info (from your VM)
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            Phone = vm.Phone,
            Notes = vm.Notes ?? string.Empty,

            DepositAmount = vm.DepositAmount,
            DepositPaid = false,
            Status = "Pending"
        };

        _context.Appointments.Add(appt);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            if (detail.Contains("UNIQUE"))
            {
                ModelState.AddModelError("", "That slot was just booked. Please choose another time.");
            }
            else
            {
                ModelState.AddModelError("", $"Booking error: {detail}");
            }
            vm.AvailableTimes = await _slots.GetAvailableSlotsAsync(vm.Date.Date, service.DurationMinutes);
            return View(vm);
        }

        // Send emails (fire-and-forget — don't block or fail the booking if email is down)
        _ = Task.Run(async () =>
        {
            var adminEmail = _config["Email:Admin"];
            var dateStr = appt.ScheduledAt.ToString("dddd, MMMM d yyyy 'at' h:mm tt");

            try
            {
                // Admin notification
                if (!string.IsNullOrWhiteSpace(adminEmail))
                {
                    await _email.SendAsync(
                        adminEmail,
                        $"New Appointment: {appt.FirstName} {appt.LastName} — {dateStr}",
                        $@"<h2>New Appointment Booked</h2>
                           <p><strong>Client:</strong> {appt.FirstName} {appt.LastName}</p>
                           <p><strong>Email:</strong> {appt.Email}</p>
                           <p><strong>Phone:</strong> {appt.Phone}</p>
                           <p><strong>Service:</strong> {service.Name}</p>
                           <p><strong>Date & Time:</strong> {dateStr}</p>
                           <p><strong>Duration:</strong> {appt.DurationMinutes} minutes</p>
                           <p><strong>Deposit:</strong> ${appt.DepositAmount:0.00}</p>
                           {(string.IsNullOrWhiteSpace(appt.Notes) ? "" : $"<p><strong>Notes:</strong> {appt.Notes}</p>")}"
                    );
                }
            }
            catch { /* Email failure should not affect the booking */ }

            try
            {
                // Client confirmation
                await _email.SendAsync(
                    appt.Email,
                    "Your Appointment is Confirmed — Matrix Health",
                    $@"<h2>Appointment Confirmed</h2>
                       <p>Hi {appt.FirstName},</p>
                       <p>Your appointment has been successfully booked. Here are your details:</p>
                       <table style='border-collapse:collapse; width:100%; max-width:500px;'>
                         <tr><td style='padding:8px; border:1px solid #ddd;'><strong>Service</strong></td><td style='padding:8px; border:1px solid #ddd;'>{service.Name}</td></tr>
                         <tr><td style='padding:8px; border:1px solid #ddd;'><strong>Date & Time</strong></td><td style='padding:8px; border:1px solid #ddd;'>{dateStr}</td></tr>
                         <tr><td style='padding:8px; border:1px solid #ddd;'><strong>Duration</strong></td><td style='padding:8px; border:1px solid #ddd;'>{appt.DurationMinutes} minutes</td></tr>
                         <tr><td style='padding:8px; border:1px solid #ddd;'><strong>Deposit</strong></td><td style='padding:8px; border:1px solid #ddd;'>${appt.DepositAmount:0.00}</td></tr>
                       </table>
                       <p style='margin-top:16px;'>If you need to reschedule or have any questions, please contact us.</p>
                       <p>Thank you for choosing Matrix Health!</p>"
                );
            }
            catch { /* Email failure should not affect the booking */ }
        });

        return RedirectToAction(nameof(Deposit), new { id = appt.Id });
    }

    // GET: /Appointments/AvailableTimes?date=2026-01-17&serviceId=3
    [HttpGet]
    public async Task<IActionResult> AvailableTimes([FromQuery] string date, [FromQuery] int serviceId)
    {
        if (!DateTime.TryParse(date, out var dt))
            return BadRequest("Invalid date.");

        if (serviceId <= 0)
            return Json(new List<string>());

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);

        if (service == null)
            return Json(new List<string>());

        var times = await _slots.GetAvailableSlotsAsync(dt.Date, service.DurationMinutes);

        return Json(times.Select(t => t.ToString(@"hh\:mm")).ToList());
    }

    // GET: /Appointments/Deposit/5
    [HttpGet]
    public async Task<IActionResult> Deposit(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        return View(appt); // Views/Appointments/Deposit.cshtml
    }

    // POST: /Appointments/MarkDepositPaid/5 (TEMP button until Stripe)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDepositPaid(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.DepositPaid = true;
        appt.Status = "Confirmed";

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Confirmed), new { id = appt.Id });
    }

    // GET: /Appointments/Confirmed/5
    [HttpGet]
    public async Task<IActionResult> Confirmed(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        return View(appt); // Views/Appointments/Confirmed.cshtml
    }
}
