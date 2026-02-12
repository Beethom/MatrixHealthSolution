using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Service
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 10000)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    // ✅ NEW: duration controls slot blocking (ex: 30, 45, 60, 90)
    [Range(15, 240)]
    public int DurationMinutes { get; set; } = 60;

    public ICollection<Appointment>? Appointments { get; set; }
}
