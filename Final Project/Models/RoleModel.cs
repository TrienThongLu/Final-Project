using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    [BsonIgnoreExtraElements]
    public class RoleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }
        [Required]
        [BsonElement("name")]
        public string? Name { get; init; }

        public static Task UniqueRoleIndex(RoleService roleService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Username' as Unique on UserModel");
            var IndexRolename = Builders<RoleModel>.IndexKeys.Ascending("phonenumber");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return roleService.roleCollection.Indexes.CreateOneAsync(new CreateIndexModel<RoleModel>(IndexRolename, IndexOptions));
        }
    }
}
