public class OrderRequest
{
    public string CustomerId { get; set; }  // Customer who places the order
    public List<OrderProductRequest> Products { get; set; }  // List of products in the order
    public string Notes { get; set; }  // Any special notes for the order
}

public class OrderProductRequest
{
    public string ProductId { get; set; }  // Product ID
    public string VendorId { get; set; }  // Vendor ID
    public int Quantity { get; set; }  // Quantity of product ordered

    // Add status to track the product's delivery status
    public ProductStatus Status { get; set; }  // Delivery status of the product (e.g., Pending, Delivered)
}
