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
        [BsonElement("sId")]
        public string sId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string StoreId { get; set; }
        public int Status { get; set; }
        public List<OrderItem> Items { get; set; }
        public long TotalPrice { get; set; }
        public int DiscountPercent { get; set; }
        public long Amount { get; set; }
        public string? TakenBy { get; set; }
        public UserDetail? CustomerInfo { get; set; }
        public int Type { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentDetail? PaymentInfo { get; set; }
        public string Note { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDone { get; set; }
        public long CreatedDate { get; set; }

        public class UserDetail
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set;}
            public string Name { get; set;}
            public string Phonenumber { get; set;}
            public string? Address { get; set;}
        }

        public class PaymentDetail
        {
            public string? MoMoRequestId { get; set; }
            public long? MoMoTransId { get; set; }
            public string? Distance { get; set;}
            public long? ShippingFee { get; set;}
            public string? PPPayId { get; set; }
            public string? PPToken { get; set; }
            public string? PPPayer { get; set; }
        }

        public static Task UniqueOrdersIdIndex(OrderService orderService, ILogger logger)
        {
            logger.LogInformation("Creating index 'sId' as Unique on OrderModel");
            var IndexsId = Builders<OrderModel>.IndexKeys.Ascending("sId");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return orderService.orderCollection.Indexes.CreateOneAsync(new CreateIndexModel<OrderModel>(IndexsId, IndexOptions));
        }
    }
}
