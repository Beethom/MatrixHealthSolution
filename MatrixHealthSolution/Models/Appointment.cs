using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    public bool IsNewPatient { get; set; }


    public string Notes { get; set; } = string.Empty;
    public string PreferredContact { get; set; } = "Email"; // Email/Text/Call
    [Required]
    public DateTime ScheduledAt { get; set; }

    // ✅ Store duration at booking time (so if service duration changes later, old bookings remain valid)
    [Range(15, 480)]
    public int DurationMinutes { get; set; } = 60;

    // deposit
    public decimal DepositAmount { get; set; } = 25m;
    public bool DepositPaid { get; set; } = false;

    public string Status { get; set; } = "Pending"; // Pending/Confirmed/Cancelled/Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ computed end time
    public DateTime EndsAt => ScheduledAt.AddMinutes(DurationMinutes);
}
