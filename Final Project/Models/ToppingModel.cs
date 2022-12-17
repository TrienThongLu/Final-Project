using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Final_Project.Services;
using MongoDB.Driver;

namespace Final_Project.Models
{
    public class ToppingModel
    {
        public ToppingModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }

        public static Task UniqueToppingIndex(ToppingService toppingService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on ToppingModel");
            var IndexName = Builders<ToppingModel>.IndexKeys.Ascending("Name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return toppingService.toppingCollection.Indexes.CreateOneAsync(new CreateIndexModel<ToppingModel>(IndexName, IndexOptions));
        }
    }
}
