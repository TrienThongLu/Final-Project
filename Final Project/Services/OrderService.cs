using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{  
    public class OrderService : IService<OrderModel>
    {
        public readonly IMongoCollection<OrderModel> oderCollection;
        public OrderService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            oderCollection = mongoClient.GetCollection<OrderModel>("Oder");
        }

        public async Task<List<OrderModel>> GetAsync()
        {
            return await oderCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<OrderModel> GetAsync(string id)
        {
            return await oderCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(OrderModel objectData)
        {
            await oderCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await oderCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, OrderModel objectData)
        {
            await oderCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }       
    }
}

