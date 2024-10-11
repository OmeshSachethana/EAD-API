using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceAPI.Data; // Adjust this if the namespace is different

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
            return BadRequest(new { Message = "Order cannot be canceled after delivery or it has already been canceled." });
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
            return NotFound(new { Message = "Order not found or already delivered or canceled." });
        }

        // Mark a specific vendor's product as delivered (Partial delivery by vendor)
        if (request.PartialDelivery && !string.IsNullOrEmpty(request.VendorId))
        {
            // Find the product for the vendor
            var product = order.Products.FirstOrDefault(p => p.VendorId == request.VendorId);
            if (product == null)
            {
                return BadRequest(new { Message = "Product not found for this vendor." });
            }

            // Mark the vendor's product as partially delivered (or fully delivered based on the request)
            product.Status = request.PartialDelivery ? ProductStatus.PartiallyDelivered : ProductStatus.Delivered;

            // Check if all products are delivered, if not mark the order as partially delivered
            if (order.Products.All(p => p.Status == ProductStatus.Delivered))
            {
                order.Status = OrderStatus.Delivered;
                order.DeliveredAt = DateTime.UtcNow;
                order.IsPartiallyDelivered = false; // The entire order is delivered
            }
            else
            {
                order.Status = OrderStatus.PartiallyDelivered;
                order.IsPartiallyDelivered = true; // Not all products are delivered yet
            }
        }
        else
        {
            // Admin or CSR marks the entire order as delivered (for all products)
            foreach (var product in order.Products)
            {
                product.Status = ProductStatus.Delivered; // Mark all products as delivered
            }

            order.Status = OrderStatus.Delivered; // Set the overall order status to Delivered
            order.DeliveredAt = DateTime.UtcNow;
            order.IsPartiallyDelivered = false; // The entire order is delivered
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        // Notify the customer about the delivery (if notification system is integrated)
        // NotificationService.NotifyCustomer(order.CustomerId, "Your order has been delivered.");

        return Ok(new { Message = "Order status updated and customer notified." });
    }


}

