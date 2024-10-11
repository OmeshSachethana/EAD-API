using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Data;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public VendorController(MongoDbContext context)
        {
            _context = context;
        }

        // Admin creates a vendor
        [Authorize(Roles = "Administrator")]
        [HttpPost("create-vendor")]
        public async Task<IActionResult> CreateVendor([FromBody] Vendor vendorRequest)
        {
            Console.WriteLine($"Vendor Request: {vendorRequest.VendorId}, {vendorRequest.VendorName}, {vendorRequest.Description}");

            if (vendorRequest == null || string.IsNullOrEmpty(vendorRequest.VendorName))
            {
                return BadRequest("Invalid vendor data.");
            }

            // Create a new Vendor object with auto-generated VendorId
            var vendor = new Vendor
            {
                VendorId = vendorRequest.VendorId,
                VendorName = vendorRequest.VendorName,
                Description = vendorRequest.Description,
                Feedback = new List<CustomerFeedback>()  // Initialize Feedback list
            };

            // Insert the vendor into the database
            await _context.Vendors.InsertOneAsync(vendor);
            return CreatedAtAction(nameof(GetVendorById), new { vendorId = vendor.VendorId }, "Vendor created successfully.");
        }

        // Retrieve vendor details
        [HttpGet("{vendorId}")]
        public async Task<IActionResult> GetVendorById(string vendorId)
        {
            Console.WriteLine($"Received request for vendor ID: {vendorId}");

            if (string.IsNullOrEmpty(vendorId))
            {
                return BadRequest("Vendor ID is required.");
            }

            var vendor = await _context.Vendors.Find(v => v.VendorId == vendorId).FirstOrDefaultAsync();
            if (vendor == null)
            {
                Console.WriteLine($"Vendor not found for ID: {vendorId}");
                return NotFound("Vendor not found.");
            }

            return Ok(new
            {
                vendor.VendorId,
                vendor.VendorName,
                vendor.Description,
                vendor.AverageRanking, // Automatically calculated
                Feedback = vendor.Feedback.Select(f => new
                {
                    f.Ranking,
                    f.Comment,
                    f.CreatedAt
                }).ToList()
            });
        }

        // Customer adds ranking and comment for the vendor
        [Authorize(Roles = "Customer")]
        [HttpPost("add-feedback/{vendorId}")]
        public async Task<IActionResult> AddFeedback(string vendorId, [FromBody] CustomerFeedback feedbackRequest)
        {
            if (feedbackRequest == null || feedbackRequest.Ranking < 1 || feedbackRequest.Ranking > 5)
            {
                return BadRequest("Invalid feedback data.");
            }

            var vendor = await _context.Vendors.Find(v => v.VendorId == vendorId).FirstOrDefaultAsync();
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            // Add feedback without storing customer-specific details
            var feedback = new CustomerFeedback
            {
                Ranking = feedbackRequest.Ranking,
                Comment = feedbackRequest.Comment,
                CreatedAt = DateTime.UtcNow
            };

            vendor.Feedback.Add(feedback);

            await _context.Vendors.ReplaceOneAsync(v => v.VendorId == vendorId, vendor);
            return Ok("Feedback added successfully.");
        }

        // Retrieve all feedback and rankings for a specific vendor
        [HttpGet("get-rankings/{vendorId}")]
        public async Task<IActionResult> GetVendorRankings(string vendorId)
        {
            var vendor = await _context.Vendors.Find(v => v.VendorId == vendorId).FirstOrDefaultAsync();
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            var feedback = vendor.Feedback.Select(f => new 
            {
                f.Ranking,
                f.Comment,
                f.CreatedAt
            }).ToList();

            return Ok(feedback);
        }

        // Customer updates their comment (ranking cannot be changed)
        [Authorize(Roles = "Customer")]
        [HttpPut("edit-comment/{vendorId}")]
        public async Task<IActionResult> EditComment(string vendorId, [FromBody] string updatedComment)
        {
            if (string.IsNullOrEmpty(updatedComment))
            {
                return BadRequest("Comment cannot be empty.");
            }

            var vendor = await _context.Vendors.Find(v => v.VendorId == vendorId).FirstOrDefaultAsync();
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            // Assuming logic for identifying which comment to update (can be extended)
            var feedback = vendor.Feedback.FirstOrDefault(f => f.Comment == updatedComment);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            feedback.Comment = updatedComment;

            await _context.Vendors.ReplaceOneAsync(v => v.VendorId == vendorId, vendor);
            return Ok("Comment updated successfully.");
        }
    }
}
