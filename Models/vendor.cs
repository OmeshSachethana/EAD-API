using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Models
{
    public class Vendor
    {
        [BsonId]
        public string VendorId { get; set; }

        public string VendorName { get; set; }  
        public string Description { get; set; } 

        [JsonIgnore]
        public List<CustomerFeedback> Feedback { get; set; } = new List<CustomerFeedback>();

        [JsonIgnore]
        public double AverageRanking
        {
            get
            {
                return Feedback.Any() ? Feedback.Average(f => f.Ranking) : 0;
            }
        }
    }

    public class CustomerFeedback
    {
        public int Ranking { get; set; }  // Customer's starred ranking (e.g., 1 to 5 stars)
        public string Comment { get; set; }  // Customer's feedback message
        public DateTime CreatedAt { get; set; }  // Timestamp of when the feedback was given
    }
}
