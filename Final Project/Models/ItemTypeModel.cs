using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class ItemTypeModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Image { get; set; }
        [Required]
        public int Style { get; set; }

        public static Task UniqueItemTypeIndex(ItemTypeService itemTypeService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on ItemTypeModel");
            var IndexName = Builders<ItemTypeModel>.IndexKeys.Ascending("Name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return itemTypeService.ItemTypeCollection.Indexes.CreateOneAsync(new CreateIndexModel<ItemTypeModel>(IndexName, IndexOptions));
        }
    }
}
