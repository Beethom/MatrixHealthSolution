using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers.Admin
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return View("~/Views/Admin/Products/Index.cshtml", products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Products/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Products/Create.cshtml", product);

            if (imageFile != null && imageFile.Length > 0)
                product.ImagePath = await SaveProductImageAsync(imageFile);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View("~/Views/Admin/Products/Edit.cshtml", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Products/Edit.cshtml", product);

            var dbProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (dbProduct == null) return NotFound();

            // Preserve existing image if none uploaded
            product.ImagePath ??= dbProduct.ImagePath;

            // Replace image if new one uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                // delete old
                if (!string.IsNullOrWhiteSpace(dbProduct.ImagePath))
                    DeleteFileIfExists(dbProduct.ImagePath);

                product.ImagePath = await SaveProductImageAsync(imageFile);
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View("~/Views/Admin/Products/Delete.cshtml", product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(product.ImagePath))
                DeleteFileIfExists(product.ImagePath);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveProductImageAsync(IFormFile file)
        {
            // basic type checks
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                throw new InvalidOperationException("Only JPG, PNG, or WEBP images are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Max image size is 5MB.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return web path
            return $"/uploads/products/{fileName}";
        }

        private void DeleteFileIfExists(string webPath)
        {
            // webPath like "/uploads/products/xxx.jpg"
            var trimmed = webPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_env.WebRootPath, trimmed);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
