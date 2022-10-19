using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class RoleService : IService<RoleModel>
    {
        public readonly IMongoCollection<RoleModel> roleCollection;

        public RoleService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            roleCollection = mongoClient.GetCollection<RoleModel>("Role");
        }

        public async Task<List<RoleModel>> GetAsync()
        {
            return await roleCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<List<RoleModel>> GetRolesAsync()
        {
            var adminRole = await RetrieveAdminRole();
            return await roleCollection.Find(r => r.Id != adminRole.Id).ToListAsync(); ;
        }

        public async Task<RoleModel> GetAsync(string id)
        {
            return await roleCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(RoleModel objectData)
        {
            await roleCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await roleCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, RoleModel objectData)
        {
            await roleCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<RoleModel> RetrieveAdminRole()
        {
            return await (roleCollection.Find(r => r.Name == "Admin").FirstOrDefaultAsync());
        }

        public async Task<RoleModel> RetrieveOnlineCustomerRole()
        {
            return await (roleCollection.Find(r => r.Name == "Online Customer").FirstOrDefaultAsync());
        }

        public async Task<List<String>> RetrieveStoreRolesId()
        {
            return roleCollection.AsQueryable().Where(r => r.Name == "Customer" || r.Name.Contains("Staff")).Select(r => r.Id).ToList();
        }

        public async Task<RoleModel> SearchRoleviaName(string name)
        {
            return await roleCollection.Find(r => r.Name == name).FirstOrDefaultAsync();
        }
    }
}
