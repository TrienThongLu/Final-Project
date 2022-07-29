using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public long Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string StatusId { get; set; }
    }
}
