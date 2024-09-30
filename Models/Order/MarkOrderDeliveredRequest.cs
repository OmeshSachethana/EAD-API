public class MarkOrderDeliveredRequest
{
    public bool PartialDelivery { get; set; }  // Indicates if only a part of the order is delivered
    public string VendorId { get; set; }  // Vendor marking their part of the order as delivered
}
