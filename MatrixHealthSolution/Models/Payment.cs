using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public decimal Amount { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Failed, Refunded

    [StringLength(50)]
    public string Method { get; set; } = "Mock"; // Mock, Stripe, Cash, etc.

    [StringLength(120)]
    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? StripeSessionId { get; set; }
public string? StripePaymentIntentId { get; set; }

}
