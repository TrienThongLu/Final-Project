using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace Final_Project.Models
{
    [BsonIgnoreExtraElements]
    public class OTPModel
    {      
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }
        [BsonElement("otp")]
        public string? OTP { get; set; }
        [BsonElement("phonenumber")]
        public string? PhoneNumber { get; set; }
        [BsonElement("type")]
        public string? Type { get; set; }
        public DateTime? ExpireAt { get; set; }

        public static Task ExpireAtTimerIndex(OTPService otpService, ILogger logger)
        {
            logger.LogInformation("Creating index 'ExpireAt' with Timer on OTPModel");
            return otpService.otpCollection.Indexes.CreateOneAsync(new CreateIndexModel<OTPModel>(
                            Builders<OTPModel>.IndexKeys.Ascending("ExpireAt"),
                            new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 30) }));
        }
    }
}
