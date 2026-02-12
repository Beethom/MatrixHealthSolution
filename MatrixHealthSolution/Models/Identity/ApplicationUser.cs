using Microsoft.AspNetCore.Identity;

namespace MatrixHealthSolution.Models.Identity;

public class ApplicationUser : IdentityUser
{
    // Extend later with extra fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DOB { get; set; }

    // Navigation
    public ICollection<Order>? Orders { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}
