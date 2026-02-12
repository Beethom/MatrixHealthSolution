using System.ComponentModel.DataAnnotations;
using MatrixHealthSolution.Models.Identity; // <-- needed for ApplicationUser


namespace MatrixHealthSolution.Models;

public class GiftCard
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(1, 10000)]
    public decimal Value { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiryDate { get; set; }

    // Navigation
    public ICollection<OrderItem>? OrderItems { get; set; }
}
