using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.RoleRequests
{
    public class AddItemRequest
    {
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public long? Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string StatusId { get; set; }
    }
}
