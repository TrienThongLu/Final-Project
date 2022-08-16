using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class ItemModel
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
        public static Task UniqueRoleIndex(ItemService ItemService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Itemname' as Unique on ItemModel");
            var IndexItemname = Builders<ItemModel>.IndexKeys.Ascending("name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return ItemService.itemCollection.Indexes.CreateOneAsync(new CreateIndexModel<ItemModel>(IndexItemname, IndexOptions));
        }
    }
}
