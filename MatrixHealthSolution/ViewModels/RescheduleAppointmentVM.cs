using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models.ViewModels;

public class RescheduleAppointmentVM
{
    public int Id { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    // service duration drives available slots
    public int DurationMinutes { get; set; }

    // UI lists
    public List<SelectListItem> Services { get; set; } = new();
    public List<TimeSpan> AvailableTimes { get; set; } = new();

    // display info
    public string? PatientName { get; set; }
    public string? PatientEmail { get; set; }
}
