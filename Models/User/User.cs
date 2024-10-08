/*
 * File: User.cs
 * Description: This class represents a user in the system, containing properties related 
 *              to the user's account, including username, email, role, password, 
 *              active status, and timestamps for creation and modification.
 * Author: Sachethana B. L. O.
 * Date: 02/10/2024
 * 
 * This class is used for MongoDB document mapping and includes data annotations for validation 
 * and automatic timestamp handling.
 */

using MongoDB.Bson; 
using MongoDB.Bson.Serialization.Attributes; 
using System.ComponentModel.DataAnnotations; 

public class User
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)] // Represents the Id as an ObjectId type in BSON
    public string? Id { get; set; }  // Nullable Id to let MongoDB auto-generate if not provided

    [Required] // Indicates that this property is required for the model to be valid
    public required string Username { get; set; } // The user's username

    [Required] // Indicates that this property is required for the model to be valid
    public required string Email { get; set; } // The user's email address

    [Required] // Indicates that this property is required for the model to be valid
    public required string Role { get; set; } // The user's role (e.g., Admin, Vendor, Customer)

    [Required] // Indicates that this property is required for the model to be valid
    public required string Password { get; set; } // The user's password

    public bool IsActive { get; set; } = true; // Indicates if the user's account is active; defaults to true

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Specifies that this property should be stored as UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Automatically set when the user is created

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Specifies that this property should be stored as UTC
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;  // Automatically updated on modification
}
