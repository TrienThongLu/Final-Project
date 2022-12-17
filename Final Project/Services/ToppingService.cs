using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class ToppingService : IService<ToppingModel>
    {
        public readonly IMongoCollection<ToppingModel> toppingCollection;
        public ToppingService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            toppingCollection = mongoClient.GetCollection<ToppingModel>("Topping");
        }

        public async Task<List<ToppingModel>> GetAsync()
        {
            return await toppingCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<ToppingModel> GetAsync(string id)
        {
            return await toppingCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ToppingModel objectData)
        {
            await toppingCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await toppingCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, ToppingModel objectData)
        {
            await toppingCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<ToppingModel> SearchToppingviaName(string Name)
        {
            return await toppingCollection.Find(r => r.Name == Name).FirstOrDefaultAsync();
        }
    }
}
