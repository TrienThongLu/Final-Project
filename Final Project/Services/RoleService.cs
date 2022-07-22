using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class RoleService
    {
        public readonly IMongoCollection<RoleModel> roleCollection;

        public RoleService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("Final Project");
            roleCollection = mongoClient.GetCollection<RoleModel>("Role");
        }
    }
}
