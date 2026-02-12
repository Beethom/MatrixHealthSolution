using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers.Admin
{
    [Area("Admin")]
    //[Route("Admin/[controller]/[action]")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View("~/Views/Admin/Services/Index.cshtml", services);
        }

       [HttpGet]
public IActionResult Create()
{
    return View("~/Views/Admin/Services/Create.cshtml");
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Service service)
{
    if (!ModelState.IsValid)
    {
        // Show validation errors back in the form
        return View("~/Views/Admin/Services/Create.cshtml", service);
    }

    _context.Services.Add(service);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View("~/Views/Admin/Services/Edit.cshtml", service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Services/Edit.cshtml", service);

            _context.Services.Update(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View("~/Views/Admin/Services/Delete.cshtml", service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
