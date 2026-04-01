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
    public class GiftCardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiftCardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.GiftCards
                .OrderByDescending(g => g.Id)
                .ToListAsync();

            return View("~/Views/Admin/GiftCards/Index.cshtml", items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/GiftCards/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiftCard giftCard)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/GiftCards/Create.cshtml", giftCard);

            _context.GiftCards.Add(giftCard);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var giftCard = await _context.GiftCards.FindAsync(id);
            if (giftCard == null) return NotFound();

            return View("~/Views/Admin/GiftCards/Edit.cshtml", giftCard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GiftCard giftCard)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/GiftCards/Edit.cshtml", giftCard);

            _context.GiftCards.Update(giftCard);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var giftCard = await _context.GiftCards.FindAsync(id);
            if (giftCard == null) return NotFound();

            return View("~/Views/Admin/GiftCards/Delete.cshtml", giftCard);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giftCard = await _context.GiftCards.FindAsync(id);
            if (giftCard == null) return NotFound();

            _context.GiftCards.Remove(giftCard);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
