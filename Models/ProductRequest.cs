public class ProductRequest
{
    public required string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; } // New property for description
    public required int Quantity { get; set; } // New property for quantity
    public required decimal Price { get; set; } // New property for price
    public string ImageUrl { get; set; } // New property for image URL
}
