using Final_Project.Requests.OrderRequests;
using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class OrderModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Status { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public long TotalPrice { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
