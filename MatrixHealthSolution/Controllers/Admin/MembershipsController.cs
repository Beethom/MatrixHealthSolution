using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers.Admin
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class MembershipsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembershipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Memberships/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ✅ Load first, then sort in memory (SQLite can't ORDER BY decimal reliably)
            var items = await _context.Memberships
                .AsNoTracking()
                .ToListAsync();

            items = items
                .OrderBy(m => m.Price)
                .ToList();

            return View("~/Views/Admin/Memberships/Index.cshtml", items);
        }

        // GET: /Admin/Memberships/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Memberships/Create.cshtml");
        }

        // POST: /Admin/Memberships/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Membership membership)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Memberships/Create.cshtml", membership);

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Memberships/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null) return NotFound();

            return View("~/Views/Admin/Memberships/Edit.cshtml", membership);
        }

        // POST: /Admin/Memberships/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Membership membership)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Memberships/Edit.cshtml", membership);

            _context.Memberships.Update(membership);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Memberships/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null) return NotFound();

            return View("~/Views/Admin/Memberships/Delete.cshtml", membership);
        }

        // POST: /Admin/Memberships/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null) return NotFound();

            _context.Memberships.Remove(membership);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
