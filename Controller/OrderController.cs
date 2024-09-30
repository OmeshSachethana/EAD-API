using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly MongoDbContext _context;

    public OrdersController(MongoDbContext context)
    {
        _context = context;
    }

    // Create a new customer order (by vendors or customers)
    [Authorize(Roles = "Vendor, Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Products = request.Products.Select(p => new OrderProductRequest
            {
                ProductId = p.ProductId,
                VendorId = p.VendorId,
                Quantity = p.Quantity,
                Status = ProductStatus.Pending  // Set initial status to Pending
            }).ToList(),
            Status = OrderStatus.Processing,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
        };

        await _context.Orders.InsertOneAsync(order);
        return Ok(order);
    }

    // Update order details before the order is dispatched
    [Authorize(Roles = "Vendor, Administrator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderUpdateRequest request)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status == OrderStatus.Processing && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest("Order cannot be updated after dispatch or it has been canceled.");
        }

        // Update the products and notes from the request
        order.Products = request.Products;  // Update the products list
        order.Notes = request.Notes;  // Update any notes or instructions
        order.UpdatedAt = DateTime.UtcNow;  // Update the timestamp for when the order is modified

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);  // Save the updated order

        return Ok(order);  // Return the updated order as a response
    }

    // Cancel the order before it's dispatched
    [Authorize(Roles = "Vendor, Administrator, Customer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(string id)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status == OrderStatus.Processing && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest("Order cannot be canceled after dispatch or it has been canceled.");
        }

        order.Status = OrderStatus.Cancelled;
        order.IsCancelled = true;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        // Notify customer about cancellation (via notification service)
        // NotificationService.NotifyCustomer(order.CustomerId, "Your order has been canceled.");

        return Ok(new { Message = "Order canceled successfully." });
    }

    // Get order status for a specific order
    [Authorize(Roles = "Vendor, Administrator, Customer, CSR")]
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetOrderStatus(string id)
    {
        var order = await _context.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null)
        {
            return NotFound("Order not found.");
        }

        return Ok(new { Status = order.Status });
    }

    [Authorize(Roles = "Vendor, Administrator")]
    [HttpPut("{id}/ship")]
    public async Task<IActionResult> MarkOrderAsShipped(string id)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status == OrderStatus.Processing).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest("Order cannot be marked as shipped. It may be cancelled or already shipped/delivered.");
        }

        order.Status = OrderStatus.Shipped;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        return Ok(new { Message = "Order marked as shipped." });
    }

}

