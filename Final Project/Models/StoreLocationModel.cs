using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Final_Project.Models
{
    public class StoreLocationModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }
        public string Address { get; set; }
        public Position Positions { get; set; }
        public int StartTime { get; set; }
        public int CloseTime { get; set; }

        public static Task UniqueStoreIndex(StoreLocationService storeService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on StoreLocationModel");
            var IndexxName = Builders<StoreLocationModel>.IndexKeys.Ascending("Name");
            var IndexxOptions = new CreateIndexOptions() { Unique = true };
            return storeService.StoreCollection.Indexes.CreateOneAsync(new CreateIndexModel<StoreLocationModel>(IndexxName, IndexxOptions));
        }
    }
    public record Position(double lat , double lng);
}
