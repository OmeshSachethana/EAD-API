/*
 * File: ProductsController.cs
 * Description: This controller handles the management of products, including creation, retrieval, updating, 
 *              deletion, and activation/deactivation of products.
 * Author: Sachethana B. L. O
 * Date: 02/10/2024
 * 
 * This file contains various API endpoints for managing products in the system using MongoDB as the database.
 * The endpoints are restricted by user roles: Vendor, Administrator, and Customer.
 */

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

    // Constructor: Initializes the ProductsController with the given MongoDbContext.
    public ProductsController(MongoDbContext context)
    {
        _context = context;
    }

    // POST: api/products
    // This method allows a vendor to create a new product.
    [Authorize(Roles = "Vendor")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
    {
        // Validate the incoming product request
        if (request == null)
        {
            return BadRequest("Product request cannot be null.");
        }

        // Retrieve the user ID from the current authenticated user
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        // Create a new product object based on the request
        var product = new Product
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description, // Set the description
            Quantity = request.Quantity, // Set the quantity
            Price = request.Price, // Set the price
            ImageUrl = request.ImageUrl, // Set the image URL
            VendorId = userIdClaim // Associate product with the current vendor
        };

        // Insert the new product into the database
        await _context.Products.InsertOneAsync(product);
        return Ok(product);
    }

    // GET: api/products
    // This method allows a customer, vendor, or administrator to retrieve products.
    [Authorize(Roles = "Vendor, Administrator, Customer")]
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        // Retrieve the user ID from the current authenticated user
        var vendorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(vendorIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        // Check user roles
        var isCustomer = User.IsInRole("Customer");
        var isVendor = User.IsInRole("Vendor");
        var isAdmin = User.IsInRole("Administrator");

        // If the user is a customer, return only active products
        if (isCustomer)
        {
            var activeProducts = await _context.Products.Find(p => p.IsActive == true).ToListAsync();
            return Ok(activeProducts);
        }

        // If the user is a vendor, return only their products
        if (isVendor)
        {
            var vendorProducts = await _context.Products.Find(p => p.VendorId == vendorIdClaim).ToListAsync();
            return Ok(vendorProducts);
        }

        // If the user is an admin, return all products
        if (isAdmin)
        {
            var allProducts = await _context.Products.Find(_ => true).ToListAsync(); // Get all products
            return Ok(allProducts);
        }

        return Forbid(); // In case of an unknown role
    }


    // GET: api/products/{id}
    // This method allows a vendor or administrator to retrieve a product by its ID.
    [Authorize(Roles = "Vendor, Administrator")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        // Find the product by its ID in the database
        var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // PUT: api/products/{id}
    // This method allows a vendor or administrator to update an existing product.
    [Authorize(Roles = "Vendor, Administrator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductRequest request)
    {
        // Retrieve the user ID from the current authenticated user
        var vendorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(vendorIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        // Find the product by its ID and ensure it belongs to the current vendor
        var product = await _context.Products.Find(p => p.Id == id && p.VendorId == vendorIdClaim).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        // Update the product with the new data from the request
        product.Name = request.Name;
        product.Category = request.Category;
        product.Description = request.Description; // Update the description
        product.Quantity = request.Quantity; // Update the quantity
        product.Price = request.Price; // Update the price
        product.ImageUrl = request.ImageUrl; // Update the image URL

        // Replace the old product data with the updated data
        await _context.Products.ReplaceOneAsync(p => p.Id == id, product);
        return Ok(product);
    }

    // DELETE: api/products/{id}
    // This method allows a vendor to delete one of their products.
    [Authorize(Roles = "Vendor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        // Retrieve the user ID from the current authenticated user
        var vendorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(vendorIdClaim))
        {
            return Unauthorized("User ID not found.");
        }

        // Attempt to delete the product owned by the vendor
        var result = await _context.Products.DeleteOneAsync(p => p.Id == id && p.VendorId == vendorIdClaim);
        if (result.DeletedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }

    // PUT: api/products/{id}/activate
    // This method allows an administrator to activate a product.
    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateProduct(string id)
    {
        // Set the IsActive flag of the product to true
        var update = Builders<Product>.Update.Set(p => p.IsActive, true);
        var result = await _context.Products.UpdateOneAsync(p => p.Id == id, update);

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }

    // PUT: api/products/{id}/deactivate
    // This method allows an administrator to deactivate a product.
    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(string id)
    {
        // Set the IsActive flag of the product to false
        var update = Builders<Product>.Update.Set(p => p.IsActive, false);
        var result = await _context.Products.UpdateOneAsync(p => p.Id == id, update);

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return Ok();
    }
}
