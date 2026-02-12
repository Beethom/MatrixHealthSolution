using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MatrixHealthSolution.Models.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    // ✅ File upload
    public IFormFile? ImageFile { get; set; }

    // Existing image (for edit)
    public string? ExistingImagePath { get; set; }
}
