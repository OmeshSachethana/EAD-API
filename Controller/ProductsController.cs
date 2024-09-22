using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Add this line

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

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Use NameIdentifier for the ID
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID not found.");
        }
        Console.WriteLine($"User ID from claims: {userIdClaim}");

        var product = new Product
        {
            Name = request.Name,
            Category = request.Category,
            VendorId = userIdClaim  // Associate the product with the vendor
        };

        await _context.Products.InsertOneAsync(product);
        return Ok(product);
    }



    [Authorize(Roles = "Vendor")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductRequest request)
    {
        var product = await _context.Products.Find(p => p.Id == id && p.VendorId == User.FindFirst("sub").Value).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        product.Name = request.Name;
        product.Category = request.Category;
        
        await _context.Products.ReplaceOneAsync(p => p.Id == id, product);
        return Ok(product);
    }

    [Authorize(Roles = "Vendor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _context.Products.DeleteOneAsync(p => p.Id == id && p.VendorId == User.FindFirst("sub").Value);
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
