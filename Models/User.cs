using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public string Id { get; set; }  // Change to string for MongoDB ObjectId

    [Required]
    public string Username { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Role { get; set; }

    public bool IsActive { get; set; } = true;
}
