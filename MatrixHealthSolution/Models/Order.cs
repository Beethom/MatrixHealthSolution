using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Order
{
    public int Id { get; set; }

    // Customer (guest checkout supported)
    [Required, StringLength(80)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(80)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [StringLength(30)]
    public string? Phone { get; set; }

    // Shipping
    [Required, StringLength(200)]
    public string Address1 { get; set; } = "";

    [StringLength(200)]
    public string? Address2 { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; } = "";

    [Required, StringLength(50)]
    public string State { get; set; } = "";

    [Required, StringLength(20)]
    public string Zip { get; set; } = "";

    [StringLength(50)]
    public string Country { get; set; } = "USA";

    // Totals
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    // Status
    [StringLength(30)]
    public string Status { get; set; } = "Pending"; // Pending, Paid, Shipped, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Tracking (optional)
    [StringLength(80)]
    public string? TrackingNumber { get; set; }
    // Navigation
    public List<OrderItem> Items { get; set; } = new();
    public Payment? Payment { get; set; }

    public string? StripeSessionId { get; set; }
public string? StripePaymentIntentId { get; set; }
public DateTime? PaidAtUtc { get; set; }

}
