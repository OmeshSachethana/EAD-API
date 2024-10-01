using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }  // Nullable Id to let MongoDB auto-generate

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Role { get; set; }

    [Required]
    public required string Password { get; set; }  // Added Password field

    public bool IsActive { get; set; } = true;
}
