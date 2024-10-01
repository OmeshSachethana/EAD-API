using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly MongoDbContext _context;

    public ProductsController(MongoDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Vendor")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
    {
        if (request == null)
        {
            return BadRequest("Product request cannot be null.");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        var product = new Product
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description, // Set the description
            Quantity = request.Quantity, // Set the quantity
            Price = request.Price, // Set the price
            ImageUrl = request.ImageUrl, // Set the image URL
            VendorId = userIdClaim 
        };

        await _context.Products.InsertOneAsync(product);
        return Ok(product);
    }

    [Authorize(Roles = "Vendor, Administrator")]
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _context.Products.Find(_ => true).ToListAsync();
        return Ok(products);
    }

    [Authorize(Roles = "Vendor, Administrator")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [Authorize(Roles = "Vendor, Administrator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductRequest request)
    {
        var vendorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(vendorIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        var product = await _context.Products.Find(p => p.Id == id && p.VendorId == vendorIdClaim).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        product.Name = request.Name;
        product.Category = request.Category;
        product.Description = request.Description; // Update the description
        product.Quantity = request.Quantity; // Update the quantity
        product.Price = request.Price; // Update the price
        product.ImageUrl = request.ImageUrl; // Update the image URL

        await _context.Products.ReplaceOneAsync(p => p.Id == id, product);
        return Ok(product);
    }

    [Authorize(Roles = "Vendor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var vendorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(vendorIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        var result = await _context.Products.DeleteOneAsync(p => p.Id == id && p.VendorId == vendorIdClaim);
        if (result.DeletedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateProduct(string id)
    {
        var update = Builders<Product>.Update.Set(p => p.IsActive, true);
        var result = await _context.Products.UpdateOneAsync(p => p.Id == id, update);

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(string id)
    {
        var update = Builders<Product>.Update.Set(p => p.IsActive, false);
        var result = await _context.Products.UpdateOneAsync(p => p.Id == id, update);

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }
}
