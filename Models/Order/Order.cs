using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string CustomerId { get; set; }  // References the Customer making the order
    public List<OrderProductRequest> Products { get; set; }  // List of products in the order
    public OrderStatus Status { get; set; }  // Order status (Processing, Shipped, Delivered, etc.)

    public string Notes { get; set; }  // Any additional notes for the order

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // When the order was created
    public DateTime? UpdatedAt { get; set; }  // When the order was last updated
    public DateTime? DispatchedAt { get; set; }  // When the order was dispatched
    public DateTime? DeliveredAt { get; set; }  // When the order was delivered

    public bool IsCancelled { get; set; } = false;  // Tracks if the order was canceled

    public bool IsPayed { get; set; } = false;  // Tracks if the order was canceled
    
    // Add the cancellation note for when an order is canceled
    public string CancellationNote { get; set; }  // Reason for cancellation provided by CSR/Admin

    // Add a flag to track partial deliveries
    public bool IsPartiallyDelivered { get; set; } = false;  // Whether the order is partially delivered
}
