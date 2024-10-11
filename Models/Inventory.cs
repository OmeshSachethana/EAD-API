using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceAPI.Models
{
    public class Inventory
    {
        [BsonId] // This attribute indicates that this property is the document ID
        public ObjectId Id { get; set; }          // Unique identifier for the inventory item

        public string ProductId { get; set; }      // Product ID
        public int QuantityAvailable { get; set; } // Available stock quantity
        public string VendorId { get; set; }       // Vendor who owns the stock

        // Define a threshold for low stock
        private const int LowStockThreshold = 10;  // Adjust as needed

        // Method to check if stock is below the threshold
        public bool IsLowStock()
        {
            return QuantityAvailable <= LowStockThreshold;
        }
    }
}
