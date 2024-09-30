using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class CustomerOrder
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string CustomerId { get; set; }  // Customer who made the order
    public List<OrderProductRequest> Products { get; set; }  // Products in the order
    public OrderStatus Status { get; set; }  // Current status of the order

    public string CancellationNote { get; set; }  // Cancellation note provided by CSR/Admin
    public bool IsCancelled { get; set; } = false;  // Flag to check if the order is cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Order creation time
    public DateTime? UpdatedAt { get; set; }  // Last update time
    public DateTime? DispatchedAt { get; set; }  // Dispatch time
    public DateTime? DeliveredAt { get; set; }  // Delivery time

    public bool IsPartiallyDelivered { get; set; } = false;  // Whether order is partially delivered
}
