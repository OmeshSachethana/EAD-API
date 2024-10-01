public class OrderUpdateRequest
{
    public List<OrderProductRequest> Products { get; set; }  // List of products to update
    public string Notes { get; set; }  // Any additional notes for the order
}
