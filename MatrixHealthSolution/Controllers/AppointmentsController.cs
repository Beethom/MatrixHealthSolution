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

    public AppointmentsController(ApplicationDbContext context, AppointmentSlotService slots)
    {
        _context = context;
        _slots = slots;
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
            Notes = vm.Notes,

            DepositAmount = vm.DepositAmount,
            DepositPaid = false,
            Status = "Pending"
        };

        _context.Appointments.Add(appt);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            ModelState.AddModelError("", "That slot was just booked. Please choose another time.");
            vm.AvailableTimes = await _slots.GetAvailableSlotsAsync(vm.Date.Date, service.DurationMinutes);
            return View(vm);
        }

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
