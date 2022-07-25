using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Final_Project.Models
{
    [BsonIgnoreExtraElements]
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }
        [Required]
        [BsonElement("fullname")]
        public string Fullname { get; set; }
        [Required]
        [BsonElement("phonenumber")]
        public long PhoneNumber { get; set; }
        [Required]
        [JsonIgnore]
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("age")]
        public int? Age { get; set; }
        [BsonElement("addresses")]
        public List<String>? Addresses { get; set; }
        [BsonElement("gender")]
        [BsonDefaultValue(0)]
        public int Gender { get; set; }
        [BsonElement("dob")]
        public long? DoB { get; set; }
        [BsonElement("ranking")]
        [BsonDefaultValue("Silver")]
        public string Ranking { get; set; }
        [BsonElement("point")]
        [BsonDefaultValue(0)]
        public long Point { get; set; }
        [BsonElement("isBanned")]
        [BsonDefaultValue(false)]
        [JsonIgnore]
        public bool IsBanned { get; set; }
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? RoleId { get; set; }

        public static Task UniqueUsernameIndex(UserService userService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Username' as Unique on UserModel");
            var IndexUsername = Builders<UserModel>.IndexKeys.Ascending("phonenumber");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return userService.userCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(IndexUsername, IndexOptions));
        }
    }
}
