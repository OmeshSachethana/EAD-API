using System.ComponentModel.DataAnnotations;

public class OrderRequest
{
    [Required(ErrorMessage = "CustomerId is required.")]
    public string CustomerId { get; set; }  // Customer who places the order

    [Required(ErrorMessage = "Products are required.")]
    [MinLength(1, ErrorMessage = "At least one product must be included.")]
    public List<OrderProductRequest> Products { get; set; }  // List of products in the order

    public string Notes { get; set; }  // Any special notes for the order
}

public class OrderProductRequest
{
    [Required(ErrorMessage = "ProductId is required.")]
    public string ProductId { get; set; }  // Product ID

    [Required(ErrorMessage = "VendorId is required.")]
    public string VendorId { get; set; }  // Vendor ID

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }  // Quantity of product ordered

    // Add status to track the product's delivery status
    public ProductStatus Status { get; set; }  // Delivery status of the product (e.g., Pending, Delivered)
}
