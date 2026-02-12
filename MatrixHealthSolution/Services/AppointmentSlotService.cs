using MatrixHealthSolution.Data;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Services;

public class AppointmentSlotService
{
    private readonly ApplicationDbContext _context;

    // Default business hours (when no override exists)
    private static readonly TimeOnly DefaultOpen = new(9, 0);
    private static readonly TimeOnly DefaultClose = new(17, 0);

    public AppointmentSlotService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Public/Admin: build available slots for a date + duration
    public async Task<List<TimeSpan>> GetAvailableSlotsAsync(
        DateTime date,
        int durationMinutes,
        int? excludeAppointmentId = null)
    {
        var d = DateOnly.FromDateTime(date);

        var ov = await _context.ScheduleOverrides
            .FirstOrDefaultAsync(x => x.Date == d);

        if (ov?.IsClosed == true)
            return new List<TimeSpan>();

        var open = ov?.OpenTime ?? DefaultOpen;
        var close = ov?.CloseTime ?? DefaultClose;

        var results = new List<TimeSpan>();
        var cursor = open.ToTimeSpan();

        while (cursor + TimeSpan.FromMinutes(durationMinutes) <= close.ToTimeSpan())
        {
            var start = date.Date.Add(cursor);
            var end = start.AddMinutes(durationMinutes);

            var hasOverlap = await _context.Appointments.AnyAsync(a =>
                a.Status != "Cancelled" &&
                (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value) &&
                start < a.ScheduledAt.AddMinutes(a.DurationMinutes) &&
                end > a.ScheduledAt
            );

            if (!hasOverlap)
                results.Add(cursor);

            cursor = cursor.Add(TimeSpan.FromMinutes(30)); // step size
        }

        return results;
    }
}
