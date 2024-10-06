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

    // Fetch all orders (Authorized for Admins and Vendors)
    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders.Find(_ => true).ToListAsync(); // Fetch all orders from the database
        if (orders == null || !orders.Any())
        {
            return NotFound(new { Message = "No orders found." });
        }

        return Ok(new
        {
            Message = "Orders fetched successfully.",
            Orders = orders
        });
    }

    // Create a new customer order (by vendors or customers)
    [Authorize(Roles = "Vendor, Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        // Validate the request
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                Message = "Invalid request data.",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        // Proceed with order creation if valid
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
            UpdatedAt = DateTime.UtcNow,
        };

        await _context.Orders.InsertOneAsync(order);

        return Ok(new
        {
            Message = "Order created successfully.",
            Order = order
        });
    }

    // Update order details before the order is dispatched
    [Authorize(Roles = "Vendor, Administrator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderUpdateRequest request)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status == OrderStatus.Processing && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest(new { Message = "Order cannot be updated after dispatch or it has been canceled." });
        }

        // Update the products and notes from the request
        order.Products = request.Products;  // Update the products list
        order.Notes = request.Notes;        // Update any notes or instructions
        order.UpdatedAt = DateTime.UtcNow;  // Update the timestamp for when the order is modified

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);  // Save the updated order

        return Ok(new
        {
            Message = "Order updated successfully.",
            Order = order  // Return the updated order as part of the response
        });
    }

    // Cancel the order before it's dispatched
    [Authorize(Roles = "Vendor, Administrator, Customer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(string id)
    {
        var order = await _context.Orders.Find(o => o.Id == id && o.Status == OrderStatus.Processing && !o.IsCancelled).FirstOrDefaultAsync();
        if (order == null)
        {
            return BadRequest(new { Message = "Order cannot be updated after dispatch or it has been canceled." });
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
            return BadRequest(new { Message = "Order not found." });
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
            return BadRequest(new { Message = "Order cannot be marked as shipped. It may be cancelled or already shipped/delivered." });
        }

        order.Status = OrderStatus.Shipped;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.Orders.ReplaceOneAsync(o => o.Id == id, order);

        return Ok(new { Message = "Order marked as shipped." });
    }

}

