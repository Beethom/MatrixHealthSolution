using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Optional: link to Identity user if needed
    public string? IdentityUserId { get; set; }
}
