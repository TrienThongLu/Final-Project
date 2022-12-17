using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class ItemTypeService : IService<ItemTypeModel>
    {        
            public readonly IMongoCollection<ItemTypeModel> ItemTypeCollection;

            public ItemTypeService(IConfiguration configuration)
            {
                var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            ItemTypeCollection = mongoClient.GetCollection<ItemTypeModel>("Type");
            }

            public async Task<List<ItemTypeModel>> GetAsync()
            {
                return await ItemTypeCollection.Find(_ => true).SortByDescending(x=>x.Style).ThenBy(t=>t.Name).ToListAsync(); 
            }

            public async Task<ItemTypeModel> GetAsync(string id)
            {
                return await ItemTypeCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
            }
            public async Task<ItemTypeModel> SearchTypeviaName(string name)
            {
                return await ItemTypeCollection.Find(r => r.Name == name).FirstOrDefaultAsync();
            }

            public async Task CreateAsync(ItemTypeModel objectData)
            {
                await ItemTypeCollection.InsertOneAsync(objectData);
            }

            public async Task DeleteAsync(string id)
            {
                await ItemTypeCollection.DeleteOneAsync(r => r.Id == id);
            }

            public async Task UpdateAsync(string id, ItemTypeModel objectData)
            {
                await ItemTypeCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
            }
    }
}
