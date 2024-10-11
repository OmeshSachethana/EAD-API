using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EcommerceAPI.Data;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public InventoryController(MongoDbContext context)
        {
            _context = context;
        }

        // Vendor adds a product and defines its quantity (stock management)
        [Authorize(Roles = "admin")]
        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] Inventory inventoryRequest)
        {
            if (inventoryRequest == null || string.IsNullOrEmpty(inventoryRequest.ProductId))
            {
                return BadRequest("Invalid product data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var inventory = new Inventory
            {
                ProductId = inventoryRequest.ProductId,
                VendorId = userIdClaim,
                QuantityAvailable = inventoryRequest.QuantityAvailable
            };

            await _context.Inventory.InsertOneAsync(inventory);
            return CreatedAtAction(nameof(CheckStock), new { productId = inventoryRequest.ProductId }, "Product added with stock.");
        }

        // Vendor updates the stock of a product
        [Authorize(Roles = "admin")]
        [HttpPut("update-product")]
        public async Task<IActionResult> UpdateStock([FromBody] Inventory inventoryRequest)
        {
            if (inventoryRequest == null || string.IsNullOrEmpty(inventoryRequest.ProductId))
            {
                return BadRequest("Invalid product data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var inventory = await _context.Inventory
                .Find(i => i.ProductId == inventoryRequest.ProductId && i.VendorId == userIdClaim)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return NotFound("Product not found.");
            }

            inventory.QuantityAvailable += inventoryRequest.QuantityAvailable;
            await _context.Inventory.ReplaceOneAsync(i => i.ProductId == inventoryRequest.ProductId && i.VendorId == userIdClaim, inventory);

            return Ok("Stock updated.");
        }

        // Vendor checks the stock of a product
        [Authorize(Roles = "admin")]
        [HttpGet("check-stock/{productId}")]
        public async Task<IActionResult> CheckStock(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest("Product ID is required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var inventory = await _context.Inventory.Find(i => i.ProductId == productId && i.VendorId == userIdClaim).FirstOrDefaultAsync();
            if (inventory == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(new { productId, inventory.QuantityAvailable });
        }

        // Vendor removes stock (cannot remove stock if there are pending orders)
        [Authorize(Roles = "admin")]
        [HttpDelete("remove-stock/{productId}")]
        public async Task<IActionResult> RemoveStock(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest("Product ID is required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var inventory = await _context.Inventory.Find(i => i.ProductId == productId && i.VendorId == userIdClaim).FirstOrDefaultAsync();
            if (inventory == null)
            {
                return NotFound("Product not found.");
            }

            bool hasPendingOrders = CheckForPendingOrders(productId);
            if (hasPendingOrders)
            {
                return BadRequest("Cannot remove stock, product has pending orders.");
            }

            await _context.Inventory.DeleteOneAsync(i => i.ProductId == productId && i.VendorId == userIdClaim);
            return Ok("Stock removed.");
        }

        // Vendor retrieves all stocks
        [Authorize(Roles = "admin")]
        [HttpGet("all-stocks")]
        public async Task<IActionResult> GetAllStocks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var allStocks = await _context.Inventory
                .Find(i => i.VendorId == userIdClaim)
                .ToListAsync();

            if (allStocks == null || allStocks.Count == 0)
            {
                return NotFound("No stocks found for this vendor.");
            }

            return Ok(allStocks);
        }

        // Vendor retrieves low stock products
        [Authorize(Roles = "admin")]
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts(int threshold = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found.");
            }

            var lowStockProducts = await _context.Inventory
                .Find(i => i.VendorId == userIdClaim && i.QuantityAvailable < threshold)
                .ToListAsync();

            if (lowStockProducts == null || lowStockProducts.Count == 0)
            {
                return NotFound("No low stock products found for this vendor.");
            }

            return Ok(lowStockProducts);
        }

        // Mocked method to check for pending orders
        private bool CheckForPendingOrders(string productId)
        {
            // This should be connected to the actual orders collection
            return false; // Mocked as false for now
        }
    }
}
