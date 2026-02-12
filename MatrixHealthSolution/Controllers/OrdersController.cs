using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;
using MatrixHealthSolution.Models.ViewModels;
using MatrixHealthSolution.Helpers;

namespace MatrixHealthSolution.Controllers;

public class OrdersController : Controller
{
    private const string CartKey = "CART";
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public OrdersController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // GET: /Orders/Checkout
    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();

        if (cart.Items == null || cart.Items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        var vm = new CheckoutVM
        {
            Cart = cart.Items,
            Subtotal = cart.Items.Sum(x => x.Price * x.Quantity),
            ShippingCost = 10.00m,
            Tax = 0m
        };

        vm.Total = vm.Subtotal + vm.ShippingCost + vm.Tax;

        return View(vm);
    }

    // POST: /Orders/CreateStripeSession
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStripeSession(CheckoutVM vm)
    {
        if (!ModelState.IsValid)
        {
            var cartVm = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();
            vm.Cart = cartVm.Items;
            vm.Subtotal = cartVm.Items.Sum(x => x.Price * x.Quantity);
            vm.ShippingCost = 10.00m;
            vm.Tax = 0m;
            vm.Total = vm.Subtotal + vm.ShippingCost + vm.Tax;
            
            return View("Checkout", vm);
        }

        var cart = HttpContext.Session.GetObject<CartVM>(CartKey) ?? new CartVM();

        if (cart.Items == null || cart.Items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var domain = $"{Request.Scheme}://{Request.Host}";

        var lineItems = cart.Items.Select(i => new SessionLineItemOptions
        {
            Quantity = i.Quantity,
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "usd",
                UnitAmount = (long)(i.Price * 100m),
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = i.Name
                }
            }
        }).ToList();

        if (vm.ShippingCost > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(vm.ShippingCost * 100m),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Shipping"
                    }
                }
            });
        }

        if (vm.Tax > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(vm.Tax * 100m),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tax"
                    }
                }
            });
        }

        var order = new Order
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            Phone = vm.Phone,
            Address1 = vm.Address1,
            Address2 = vm.Address2,
            City = vm.City,
            State = vm.State,
            Zip = vm.Zip,
            Country = vm.Country ?? "USA",
            Subtotal = vm.Subtotal,
            ShippingCost = vm.ShippingCost,
            Tax = vm.Tax,
            Total = vm.Total,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        foreach (var c in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.Name,
                UnitPrice = c.Price,
                Quantity = c.Quantity,
                LineTotal = c.Price * c.Quantity
            });
        }

        order.Payment = new Payment
        {
            Amount = vm.Total,
            Status = "Unpaid",
            Method = "Stripe",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = domain + $"/Orders/ThankYou?id={order.Id}&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = domain + "/Orders/Checkout",
            LineItems = lineItems,
            CustomerEmail = vm.Email,
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = order.Id.ToString()
            }
        };

        var service = new SessionService();
        Session session;
        
        try
        {
            session = await service.CreateAsync(options);
        }
        catch (StripeException ex)
        {
            TempData["Error"] = "Payment processing error. Please try again.";
            return RedirectToAction("Checkout");
        }

        order.Payment.StripeSessionId = session.Id;
        await _context.SaveChangesAsync();

        return Redirect(session.Url);
    }

    // GET: /Orders/ThankYou?id=123&session_id=cs_test_...
    [HttpGet]
    public async Task<IActionResult> ThankYou(int id, string? session_id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        // If we have a session_id and payment is still unpaid, verify with Stripe
        if (!string.IsNullOrEmpty(session_id) && order.Payment?.Status == "Unpaid")
        {
            try
            {
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
                var service = new SessionService();
                var session = await service.GetAsync(session_id);

                // If Stripe says payment is complete, update our database
                if (session.PaymentStatus == "paid")
                {
                    order.Payment.Status = "Paid";
                    order.Payment.StripePaymentIntentId = session.PaymentIntentId;
                    order.Status = "Paid";
                    await _context.SaveChangesAsync();
                }
            }
            catch (StripeException)
            {
                // If Stripe verification fails, continue showing the page
                // The webhook will update it later
            }
        }

        HttpContext.Session.Remove(CartKey);

        return View(order);
    }
}