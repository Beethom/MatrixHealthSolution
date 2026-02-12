using MatrixHealthSolution.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatrixHealthSolution.Controllers;

public class MembershipsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MembershipsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Memberships/Index
    public async Task<IActionResult> Index()
    {
      var memberships = (await _context.Memberships
        .AsNoTracking()
        .ToListAsync())
    .OrderBy(m => m.Price)
    .ToList();


        return View(memberships); // Views/Memberships/Index.cshtml
    }
}
