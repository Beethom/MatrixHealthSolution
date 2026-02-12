using System.ComponentModel.DataAnnotations;
using MatrixHealthSolution.Models;

namespace MatrixHealthSolution.Models.ViewModels;

public class CheckoutVM
{
    // Customer
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

    public string Country { get; set; } = "USA";

    // Summary (display only)
    public List<CartItem> Cart { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}
