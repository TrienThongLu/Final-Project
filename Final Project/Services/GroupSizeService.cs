using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class GroupSizeService : IService<GroupSizeModel>
    {
        public readonly IMongoCollection<GroupSizeModel> SizeCollection;

        public GroupSizeService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            SizeCollection = mongoClient.GetCollection<GroupSizeModel>("Size");
        }

        public async Task<List<GroupSizeModel>> GetAsync()
        {
            return await SizeCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<GroupSizeModel> GetAsync(string id)
        {
            return await SizeCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }
        public async Task<GroupSizeModel> SearchTypeviaName(string size)
        {
            return await SizeCollection.Find(r => r.Size == size).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(GroupSizeModel objectData)
        {
            await SizeCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await SizeCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, GroupSizeModel objectData)
        {
            await SizeCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }
    }
}
