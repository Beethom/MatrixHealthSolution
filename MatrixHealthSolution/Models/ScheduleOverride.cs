using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class ScheduleOverride
{
    public int Id { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public bool IsClosed { get; set; } = false;

    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }

    [StringLength(200)]
    public string? Note { get; set; }
}
