using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string VendorId { get; set; } // Associate product with a vendor

    public string Category { get; set; }

    public string Description { get; set; } // New property for description

    [Required]
    public int Quantity { get; set; } // New property for quantity

    [Required]
    public decimal Price { get; set; } // New property for price

    public string ImageUrl { get; set; } // New property for image URL

    public bool IsActive { get; set; } = true;  // For activation/deactivation of the product
}
