using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class ItemModel
    {
        public ItemModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public long Price { get; set; }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }
        public string Status { get; set; }
        public List<string> GroupSize { get; set; }
        public List<ToppingModel> Topping { get; set; }
        public static Task UniqueItemIndex(ItemService ItemService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on ItemModel");
            var IndexName = Builders<ItemModel>.IndexKeys.Ascending("Name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return ItemService.itemCollection.Indexes.CreateOneAsync(new CreateIndexModel<ItemModel>(IndexName, IndexOptions));
        }

        public List<string> GroupSizes { get; set; }
    }
}
