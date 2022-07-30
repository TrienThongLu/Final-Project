using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Final_Project.Models
{
    public class TokenModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Token { get; set; }
        public DateTime? ExpireAt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        public static Task ExpireAtTimerIndex(TokenService tokenService, ILogger logger)
        {
            logger.LogInformation("Creating index 'ExpireAt' with Timer on TokenModel");
            return tokenService.tokenCollection.Indexes.CreateOneAsync(new CreateIndexModel<TokenModel>(
                            Builders<TokenModel>.IndexKeys.Ascending("ExpireAt"),
                            new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 20) }));
        }
    }
}
