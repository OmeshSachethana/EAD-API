/*
 * File: ProductRequest.cs
 * Description: This class serves as a data transfer object (DTO) for creating or updating 
 *              products in the system. It contains properties that represent the necessary 
 *              details for a product, including its name, category, description, quantity, 
 *              price, and image URL.
 * Author: Sachethana B. L. O
 * Date: 02/10/2024
 * 
 * This class is used for validation and mapping incoming requests to product data.
 */

public class ProductRequest
{
    public required string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; } // New property for description
    public required int Quantity { get; set; } // New property for quantity
    public required decimal Price { get; set; } // New property for price
    public string ImageUrl { get; set; } // New property for image URL
}
