using System.Text;
using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using MatrixHealthSolution.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers.Admin;

[Area("Admin")]
[Route("Admin/[controller]/[action]")]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Default business hours (when no override exists)
    private static readonly TimeOnly DefaultOpen = new(9, 0);
    private static readonly TimeOnly DefaultClose = new(17, 0);

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ Dashboard
    // /Admin/Appointments/Index?quick=Today&status=Pending&q=bob&from=2026-01-01&to=2026-01-20
    [HttpGet]
    public async Task<IActionResult> Index(
        string? quick = null,
        string? status = null,
        string? q = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var query = _context.Appointments
            .Include(a => a.Service)
            .AsQueryable();

        var today = DateTime.Today;

        if (quick == "Today")
        {
            query = query.Where(a => a.ScheduledAt.Date == today);
        }
        else if (quick == "ThisWeek")
        {
            var start = today.AddDays(-(int)today.DayOfWeek); // Sunday start
            var end = start.AddDays(7);
            query = query.Where(a => a.ScheduledAt >= start && a.ScheduledAt < end);
        }
        else if (from.HasValue && to.HasValue)
        {
            var start = from.Value.Date;
            var end = to.Value.Date.AddDays(1);
            query = query.Where(a => a.ScheduledAt >= start && a.ScheduledAt < end);
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(a =>
                a.Email.Contains(q) ||
                a.FirstName.Contains(q) ||
                a.LastName.Contains(q) ||
                (a.FirstName + " " + a.LastName).Contains(q) ||
                a.Id.ToString() == q
            );
        }

        var items = await query
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

        ViewBag.quick = quick;
        ViewBag.status = status;
        ViewBag.q = q;
        ViewBag.from = from?.ToString("yyyy-MM-dd");
        ViewBag.to = to?.ToString("yyyy-MM-dd");

        return View("~/Views/Admin/Appointments/Index.cshtml", items);
    }

    // ✅ Details
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        return View("~/Views/Admin/Appointments/Details.cshtml", appt);
    }

    // ✅ Mark Deposit Paid (temp until Stripe)
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDepositPaid(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.DepositPaid = true;

        if (appt.Status == "Pending")
            appt.Status = "Confirmed";

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Confirm (only if deposit is paid)
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        if (!appt.DepositPaid)
        {
            TempData["Error"] = "Cannot confirm appointment unless deposit is paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        appt.Status = "Confirmed";
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Cancel
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ✅ Complete
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.Status = "Completed";
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ✅ Admin can change deposit amount
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDeposit(int id, decimal depositAmount)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.DepositAmount = depositAmount;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Reschedule page
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Reschedule(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var duration = appt.DurationMinutes > 0 ? appt.DurationMinutes : 60;

        var vm = new RescheduleAppointmentVM
        {
            Id = appt.Id,
            ServiceId = appt.ServiceId,
            Date = appt.ScheduledAt.Date,
            StartTime = appt.ScheduledAt.TimeOfDay,
            DurationMinutes = duration,
            PatientName = $"{appt.FirstName} {appt.LastName}".Trim(),
            PatientEmail = appt.Email,
            Services = services.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = (s.Id == appt.ServiceId)
            }).ToList()
        };

        vm.AvailableTimes = await BuildSlotsForDateAsync(vm.Date, vm.DurationMinutes, excludeAppointmentId: appt.Id);

        // Keep current selection visible even if rules changed
        if (!vm.AvailableTimes.Contains(vm.StartTime))
            vm.AvailableTimes.Insert(0, vm.StartTime);

        return View("~/Views/Admin/Appointments/Reschedule.cshtml", vm);
    }

    // ✅ Reschedule submit
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int id, RescheduleAppointmentVM vm)
    {
        if (id != vm.Id) return BadRequest();

        var appt = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        vm.Services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();

        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == vm.ServiceId);
        vm.DurationMinutes = service?.DurationMinutes > 0
            ? service.DurationMinutes
            : (appt.DurationMinutes > 0 ? appt.DurationMinutes : 60);

        vm.AvailableTimes = await BuildSlotsForDateAsync(vm.Date, vm.DurationMinutes, excludeAppointmentId: appt.Id);

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Appointments/Reschedule.cshtml", vm);

        if (!vm.AvailableTimes.Contains(vm.StartTime))
        {
            TempData["Error"] = "That time is no longer available. Please choose another slot.";
            return View("~/Views/Admin/Appointments/Reschedule.cshtml", vm);
        }

        var scheduledAt = vm.Date.Date.Add(vm.StartTime);
        var end = scheduledAt.AddMinutes(vm.DurationMinutes);

        if (await HasOverlapAsync(appt.Id, scheduledAt, end))
        {
            TempData["Error"] = "That time overlaps with another appointment. Pick another slot.";
            return View("~/Views/Admin/Appointments/Reschedule.cshtml", vm);
        }

        appt.ServiceId = vm.ServiceId;
        appt.ScheduledAt = scheduledAt;
        appt.DurationMinutes = vm.DurationMinutes;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Export CSV
    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var items = await _context.Appointments
            .Include(a => a.Service)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,Date,Time,Service,Patient,Email,DepositAmount,DepositPaid,Status");

        foreach (var a in items)
        {
            sb.AppendLine($"{a.Id},{a.ScheduledAt:yyyy-MM-dd},{a.ScheduledAt:HH:mm},{Escape(a.Service?.Name)},{Escape(a.FirstName + " " + a.LastName)},{Escape(a.Email)},{a.DepositAmount},{a.DepositPaid},{a.Status}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "appointments.csv");
    }

    // ✅ Ajax endpoint for available times (reschedule screen)
    [HttpGet]
    public async Task<IActionResult> AvailableTimes(DateTime date, int durationMinutes, int? excludeAppointmentId = null)
    {
        var slots = await BuildSlotsForDateAsync(date, durationMinutes, excludeAppointmentId);
        return Json(slots.Select(t => t.ToString(@"hh\:mm")));
    }

    // ---------------- Helpers ----------------

    private async Task<bool> HasOverlapAsync(int? excludeId, DateTime start, DateTime end)
    {
        return await _context.Appointments.AnyAsync(a =>
            a.Status != "Cancelled" &&
            (!excludeId.HasValue || a.Id != excludeId.Value) &&
            start < a.ScheduledAt.AddMinutes(a.DurationMinutes > 0 ? a.DurationMinutes : 60) &&
            end > a.ScheduledAt
        );
    }

    // ✅ ONE correct override-aware slot builder
    private async Task<List<TimeSpan>> BuildSlotsForDateAsync(DateTime date, int durationMinutes, int? excludeAppointmentId)
    {
        var d = DateOnly.FromDateTime(date);

        var ov = await _context.ScheduleOverrides
            .FirstOrDefaultAsync(x => x.Date == d);

        if (ov?.IsClosed == true) return new List<TimeSpan>();

        var open = ov?.OpenTime ?? DefaultOpen;
        var close = ov?.CloseTime ?? DefaultClose;

        var openTs = open.ToTimeSpan();
        var closeTs = close.ToTimeSpan();

        var results = new List<TimeSpan>();
        var cursor = openTs;

        while (cursor + TimeSpan.FromMinutes(durationMinutes) <= closeTs)
        {
            var start = date.Date.Add(cursor);
            var end = start.AddMinutes(durationMinutes);

            if (!await HasOverlapAsync(excludeAppointmentId, start, end))
                results.Add(cursor);

            cursor = cursor.Add(TimeSpan.FromMinutes(30));
        }

        return results;
    }

    private static string Escape(string? s)
        => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
}
