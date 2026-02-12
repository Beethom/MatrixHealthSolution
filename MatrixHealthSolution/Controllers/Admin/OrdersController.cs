using MatrixHealthSolution.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatrixHealthSolution.Models; 

namespace MatrixHealthSolution.Controllers.Admin;

[Area("Admin")]
[Route("Admin/[controller]/[action]")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ Dashboard
    // /Admin/Orders/Index?status=Paid&q=alex&from=2026-01-01&to=2026-01-31
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null, string? q = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Orders
            .Include(o => o.Payment)
            .AsQueryable();

        // date range
        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(o => o.CreatedAt >= start);
        }

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < end);
        }

        // status
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        // search by id, name, email
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();

            query = query.Where(o =>
                o.Email.Contains(q) ||
                o.FirstName.Contains(q) ||
                o.LastName.Contains(q) ||
                (o.FirstName + " " + o.LastName).Contains(q) ||
                o.Id.ToString() == q
            );
        }

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.status = status ?? "";
        ViewBag.q = q ?? "";
        ViewBag.from = from?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.to = to?.ToString("yyyy-MM-dd") ?? "";

        return View("~/Views/Admin/Orders/Index.cshtml", items);
    }

    // ✅ Details
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View("~/Views/Admin/Orders/Details.cshtml", order);
    }

    // ✅ Update Status
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        status = (status ?? "").Trim();

        var allowed = new[] { "Pending", "Paid", "Shipped", "Cancelled", "Refunded" };
        if (!allowed.Contains(status))
        {
            TempData["Error"] = "Invalid status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // small rule examples:
        // If you set Paid but payment is still Unpaid, update payment.
        if (status == "Paid")
        {
            if (order.Payment == null)
            {
                order.Payment = new Models.Payment
                {
                    OrderId = order.Id,
                    Amount = order.Total,
                    Status = "Paid",
                    Method = "Admin",
                    TransactionId = $"ADMIN-{Guid.NewGuid():N}".ToUpper(),
                    CreatedAt = DateTime.UtcNow
                };
            }
            else
            {
                order.Payment.Status = "Paid";
                order.Payment.TransactionId ??= $"ADMIN-{Guid.NewGuid():N}".ToUpper();
            }
        }

        order.Status = status;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Order status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Update Tracking Number (optional)
    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTracking(int id, string? trackingNumber)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();

        // if tracking number set, many businesses also set Shipped automatically
        if (!string.IsNullOrWhiteSpace(order.TrackingNumber) && order.Status == "Paid")
            order.Status = "Shipped";

        await _context.SaveChangesAsync();
        TempData["Success"] = "Tracking updated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
