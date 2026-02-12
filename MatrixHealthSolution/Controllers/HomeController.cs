using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Take(6)                 // show top 6 on home
                .ToListAsync();

            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Take(6)                 // show top 6 on home
                .ToListAsync();

                

            var vm = new HomeViewModel
            {
                Services = services,
                Products = products
            };

            return View(vm); // Views/Home/Index.cshtml
        }
    }
}
