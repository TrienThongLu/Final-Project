using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class UserService : IService<UserModel>
    {
        public readonly IMongoCollection<UserModel> userCollection;
        public readonly RoleService _roleService;

        public UserService(IConfiguration configuration, RoleService roleService)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            userCollection = mongoClient.GetCollection<UserModel>("User");
            this._roleService = roleService;
        }

        public async Task<List<UserModel>> GetAsync()
        {
            return await userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<UserModel> GetAsync(string id)
        {
            return await userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(UserModel objectData)
        {
            await userCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await userCollection.DeleteOneAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(string id, UserModel objectData)
        {
            await userCollection.ReplaceOneAsync(u => u.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<UserModel> GetViaPhonenumberAsync(string phonenumber)
        {
            return await userCollection.Find(u => u.PhoneNumber == phonenumber).FirstOrDefaultAsync();
        }

        public async Task<bool> AlreadyHasAdmin()
        {
            var _idRoleAdmin = await _roleService.RetrieveAdminRole();
            return await userCollection.Find(u => u.RoleId == _idRoleAdmin.Id).AnyAsync();
        }

        public async Task<bool> RoleIsUsed(string id)
        {
            return await userCollection.Find(u => u.RoleId == id).AnyAsync();
        }
    }
}
