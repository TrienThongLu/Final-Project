using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class UserService
    {
        public readonly IMongoCollection<UserModel> userCollection;

        public UserService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("Final Project");
            userCollection = mongoClient.GetCollection<UserModel>("User");
        }
    }
}
