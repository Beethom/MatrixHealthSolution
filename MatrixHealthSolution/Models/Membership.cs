using System.ComponentModel.DataAnnotations;
using MatrixHealthSolution.Models.Enums;

namespace MatrixHealthSolution.Models;

public class Membership
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    public MembershipPeriod Period { get; set; }

    public bool IsActive { get; set; } = true;
}
