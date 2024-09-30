using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[Route("api/[controller]")]
[ApiController]
public class CustomerOrdersController : ControllerBase
{
    private readonly MongoDbContext _context;

    public CustomerOrdersController(MongoDbContext context)
    {
        _context = context;
    }

    // CSR or Administrator can cancel a customer order upon request
    [Authorize(Roles = "CSR, Administrator")]
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelCustomerOrder(string id, [FromBody] CancelOrderRequest request)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status != OrderStatus.Delivered && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest("Order cannot be canceled after delivery or it has already been canceled.");
        }

        order.Status = OrderStatus.Cancelled;
        order.IsCancelled = true;
        order.UpdatedAt = DateTime.UtcNow;
        order.CancellationNote = request.Note; // Add a note explaining the cancellation

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        // Notify the customer about the cancellation
        // NotificationService.NotifyCustomer(order.CustomerId, "Your order has been canceled.");

        return Ok(new { Message = "Order canceled and customer notified." });
    }

    // CSR, Administrator, or Vendor marks the order as delivered or partially delivered
    [Authorize(Roles = "CSR, Administrator, Vendor")]
    [HttpPut("{id}/deliver")]
    public async Task<IActionResult> MarkOrderAsDelivered(string id, [FromBody] MarkOrderDeliveredRequest request)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status != OrderStatus.Delivered && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return NotFound("Order not found or already delivered or canceled.");
        }

        // Mark a specific vendor's product as delivered
        if (request.PartialDelivery && !string.IsNullOrEmpty(request.VendorId))
        {
            var product = order.Products.FirstOrDefault(p => p.VendorId == request.VendorId);
            if (product == null)
            {
                return BadRequest("Product not found for this vendor.");
            }

            // Mark this product as delivered
            product.Status = (ProductStatus)OrderStatus.PartiallyDelivered;
            order.IsPartiallyDelivered = true;
        }
        else
        {
            // Admin or CSR marks the entire order as delivered
            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.IsPartiallyDelivered = false;
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        // Notify the customer about the delivery
        // NotificationService.NotifyCustomer(order.CustomerId, "Your order has been delivered.");

        return Ok(new { Message = "Order marked as delivered and customer notified." });
    }
}

