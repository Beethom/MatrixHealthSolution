using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers.Admin;

[Area("Admin")]
[Route("Admin/[controller]/[action]")]
public class ScheduleOverridesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ScheduleOverridesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Admin/ScheduleOverrides/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _context.ScheduleOverrides
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        return View("~/Views/Admin/ScheduleOverrides/Index.cshtml", items);
    }

    // GET: /Admin/ScheduleOverrides/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new ScheduleOverride
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        return View("~/Views/Admin/ScheduleOverrides/Create.cshtml", model);
    }

    // POST: /Admin/ScheduleOverrides/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduleOverride model)
    {
        Normalize(model);

        if (!ModelState.IsValid)
            return View("~/Views/Admin/ScheduleOverrides/Create.cshtml", model);

        // prevent duplicate date entries
        if (await _context.ScheduleOverrides.AnyAsync(x => x.Date == model.Date))
        {
            ModelState.AddModelError("", "An override already exists for that date.");
            return View("~/Views/Admin/ScheduleOverrides/Create.cshtml", model);
        }

        _context.ScheduleOverrides.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/ScheduleOverrides/Edit/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.ScheduleOverrides.FindAsync(id);
        if (item == null) return NotFound();

        return View("~/Views/Admin/ScheduleOverrides/Edit.cshtml", item);
    }

    // POST: /Admin/ScheduleOverrides/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ScheduleOverride model)
    {
        Normalize(model);

        if (!ModelState.IsValid)
            return View("~/Views/Admin/ScheduleOverrides/Edit.cshtml", model);

        // prevent changing date to an existing date
        var exists = await _context.ScheduleOverrides
            .AnyAsync(x => x.Id != model.Id && x.Date == model.Date);

        if (exists)
        {
            ModelState.AddModelError("", "Another override already exists for that date.");
            return View("~/Views/Admin/ScheduleOverrides/Edit.cshtml", model);
        }

        _context.ScheduleOverrides.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/ScheduleOverrides/Delete/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ScheduleOverrides.FindAsync(id);
        if (item == null) return NotFound();

        return View("~/Views/Admin/ScheduleOverrides/Delete.cshtml", item);
    }

    // POST: /Admin/ScheduleOverrides/Delete
    [HttpPost("{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ScheduleOverrides.FindAsync(id);
        if (item == null) return NotFound();

        _context.ScheduleOverrides.Remove(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ---------------- Helpers ----------------

    private void Normalize(ScheduleOverride m)
    {
        // If closed, ignore times
        if (m.IsClosed)
        {
            m.OpenTime = null;
            m.CloseTime = null;
            return;
        }

        // If not closed, times are required
        if (m.OpenTime == null)
            ModelState.AddModelError(nameof(m.OpenTime), "Open time is required.");

        if (m.CloseTime == null)
            ModelState.AddModelError(nameof(m.CloseTime), "Close time is required.");

        if (m.OpenTime != null && m.CloseTime != null && m.OpenTime >= m.CloseTime)
        {
            ModelState.AddModelError("", "Open time must be earlier than close time.");
        }
    }
}
