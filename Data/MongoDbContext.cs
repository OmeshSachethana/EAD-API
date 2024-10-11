using MongoDB.Driver;              // Required for MongoDB interactions
using EcommerceAPI.Models;          // Make sure this matches your Models namespace
using Microsoft.Extensions.Configuration;  // Required for IConfiguration interface

namespace EcommerceAPI.Data        // Ensure your namespace aligns with your project structure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        // Constructor that takes IConfiguration to fetch MongoDB connection string
        public MongoDbContext(IConfiguration configuration)
        {
            // Fetch connection string from configuration and initialize the MongoClient
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("ecommerce_db");  // Database name, ensure it's correct
        }

        // Define collections for different models/entities
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
        public IMongoCollection<Inventory> Inventory => _database.GetCollection<Inventory>("Inventory");
        public IMongoCollection<Vendor> Vendors => _database.GetCollection<Vendor>("Vendors");
    }
}
