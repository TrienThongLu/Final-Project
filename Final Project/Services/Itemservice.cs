using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class ItemService : IService<ItemModel>
    {
        public readonly IMongoCollection<ItemModel> itemCollection;
        public ItemService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            itemCollection = mongoClient.GetCollection<ItemModel>("Item");
        }

        public async Task<List<ItemModel>> GetAsync()
        {
            return await itemCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<ItemModel> GetAsync(string id)
        {
            return await itemCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ItemModel objectData)
        {
            await itemCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await itemCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, ItemModel objectData)
        {
            await itemCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }        

        public async Task<ItemModel> SearchItemviaName(string Itemname)
        {
            return await itemCollection.Find(r => r.ItemName == Itemname).FirstOrDefaultAsync();
        }
    }
}
