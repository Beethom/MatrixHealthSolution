using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MatrixHealthSolution.Models.ViewModels;

public class BookAppointmentVM
{
    // Service + schedule
    [Required(ErrorMessage = "Please select a service.")]
    [Display(Name = "Service")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Please select a date.")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a time.")]
    [Display(Name = "Time")]
    public TimeSpan StartTime { get; set; }

    // Customer info (more professional)
    [Required, StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    [Display(Name = "Phone number")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "New patient?")]
    public bool IsNewPatient { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes / reason for visit")]
    public string? Notes { get; set; }

    // Deposit
    [Range(0, 10000)]
    [Display(Name = "Deposit")]
    public decimal DepositAmount { get; set; } = 25m;

    // Dropdowns
    public List<SelectListItem> Services { get; set; } = new();
    public List<TimeSpan> AvailableTimes { get; set; } = new();
}
