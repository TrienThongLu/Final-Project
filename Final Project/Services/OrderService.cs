using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{  
    public class OrderService : IService<OrderModel>
    {
        public readonly IMongoCollection<OrderModel> orderCollection;
        public OrderService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            orderCollection = mongoClient.GetCollection<OrderModel>("Order");
        }

        public async Task<List<OrderModel>> GetAsync()
        {
            return await orderCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<OrderModel> GetAsync(string id)
        {
            return await orderCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(OrderModel objectData)
        {
            await orderCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await orderCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, OrderModel objectData)
        {
            await orderCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }       
    }
}

