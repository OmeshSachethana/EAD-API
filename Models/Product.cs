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

    public bool IsActive { get; set; } = true;  // For activation/deactivation of the product
}
