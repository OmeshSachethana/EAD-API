// Enum for ProductStatus (add this to track product-level status)
public enum ProductStatus
{
    Pending,            // Product is not yet delivered
    PartiallyDelivered, // Part of the order has been delivered
    Delivered,          // Product has been fully delivered
    Cancelled           // Product has been canceled
}