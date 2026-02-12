using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models.ViewModels;

public class ContactVM
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(150)]
    public string Subject { get; set; } = "";

    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = "";
}
