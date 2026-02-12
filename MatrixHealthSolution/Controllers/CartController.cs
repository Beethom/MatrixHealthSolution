using MatrixHealthSolution.Data;
using MatrixHealthSolution.Helpers;
using MatrixHealthSolution.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MatrixHealthSolution.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartKey = "CART";

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Cart
    [HttpGet]
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();
        return View(cart); // Views/Cart/Index.cshtml
    }

    // POST: /Cart/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, int qty = 1)
    {
        if (qty < 1) qty = 1;
        if (qty > 10) qty = 10;

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return NotFound();

        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == id);
        if (existing != null)
        {
            existing.Quantity += qty;
            if (existing.Quantity > 10) existing.Quantity = 10;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImagePath = product.ImagePath,
                Quantity = qty
            });
        }

        HttpContext.Session.SetObject(CartKey, cart);

        TempData["Success"] = "Added to cart!";
        return RedirectToAction("Index", "Products");
    }

    // POST: /Cart/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int qty)
    {
        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (qty <= 0)
                cart.Items.Remove(item);
            else
                item.Quantity = Math.Min(10, qty);
        }

        HttpContext.Session.SetObject(CartKey, cart);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();
        cart.Items.RemoveAll(i => i.ProductId == productId);
        HttpContext.Session.SetObject(CartKey, cart);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartKey);
        return RedirectToAction(nameof(Index));
    }
}
