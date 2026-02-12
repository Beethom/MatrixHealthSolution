using System.ComponentModel.DataAnnotations;

namespace MatrixHealthSolution.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;

    // ✅ NEW
    public string? ImagePath { get; set; }
}
