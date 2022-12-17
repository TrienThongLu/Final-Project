using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Final_Project.Utils.Resources.Attributes;
using AutoMapper;

namespace Final_Project.Models
{
    [BsonIgnoreExtraElements]
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; init; }
        [BsonElement("fullname")]
        public string Fullname { get; set; }
        [BsonElement("phonenumber")]
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        [BsonElement("passwordHash")]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        [BsonElement("passwordSalt")]
        public byte[] PasswordSalt { get; set; }
        [BsonElement("addresses")]
        public List<String> Addresses { get; set; }
        [BsonElement("gender")]
        public int? Gender { get; set; }
        [BsonElement("dob")]
        public long? DoB { get; set; }
        [BsonElement("ranking")]
        public string Ranking { get; set; }
        [BsonElement("point")]
        [BsonDefaultValue(0)]
        public long Point { get; set; }
        [BsonElement("isBanned")]
        [BsonDefaultValue(false)]
        public bool IsBanned { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? RoleId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StoreId { get; set; }

        public static Task UniqueUsernameIndex(UserService userService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Username' as Unique on UserModel");
            var IndexUsername = Builders<UserModel>.IndexKeys.Ascending("phonenumber");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return userService.userCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(IndexUsername, IndexOptions));
        }
    }
}
