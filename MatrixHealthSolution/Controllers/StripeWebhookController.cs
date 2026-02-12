using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using MatrixHealthSolution.Data;
using MatrixHealthSolution.Models;

namespace MatrixHealthSolution.Controllers;

[ApiController]
[Route("stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IConfiguration config, 
        ApplicationDbContext context,
        ILogger<StripeWebhookController> logger)
    {
        _config = config;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _config["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogError("Stripe webhook secret is not configured");
            return BadRequest("Webhook secret not configured");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest();
        }

        _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session?.Id == null)
            {
                _logger.LogWarning("Checkout session is null or has no ID");
                return Ok();
            }

            var orderIdStr = session.Metadata?["orderId"];
            if (!int.TryParse(orderIdStr, out var orderId))
            {
                _logger.LogWarning("Order ID not found in session metadata. Session ID: {SessionId}", session.Id);
                return Ok();
            }

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order not found for ID: {OrderId}", orderId);
                return Ok();
            }

            // Create payment if it doesn't exist
            if (order.Payment == null)
            {
                order.Payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.Total,
                    Status = "Paid",
                    Method = "Stripe",
                    CreatedAt = DateTime.UtcNow
                };
                _logger.LogInformation("Created payment record for order {OrderId}", orderId);
            }

            // Update payment and order status
            order.Payment.Status = "Paid";
            order.Payment.Method = "Stripe";
            order.Payment.StripeSessionId = session.Id;
            order.Payment.StripePaymentIntentId = session.PaymentIntentId;
            order.Status = "Paid";

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully processed payment for order {OrderId}. Session: {SessionId}", 
                orderId, 
                session.Id
            );
        }

        return Ok();
    }
}