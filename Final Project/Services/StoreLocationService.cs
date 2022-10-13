using Final_Project.Models;
using Final_Project.Requests.Query;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class StoreLocationService : IService<StoreLocationModel>
    {
        public readonly IMongoCollection<StoreLocationModel> StoreCollection;

        public StoreLocationService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            StoreCollection = mongoClient.GetCollection<StoreLocationModel>("Store");
        }

        public async Task<List<StoreLocationModel>> GetAsync()
        {
            return await StoreCollection.Find(_ => true).ToListAsync();
        }

        public async Task<StoreLocationModel> GetAsync(string id)
        {
            return await StoreCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }
        public async Task<StoreLocationModel> SearchTypeviaName(string name)
        {
            return await StoreCollection.Find(r => r.Name == name).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(StoreLocationModel objectData)
        {
            await StoreCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await StoreCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, StoreLocationModel objectData)
        {
            await StoreCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<Object> GetAsync(StorePaginationRequest paginationRequest)
        {
            var filters = Builders<StoreLocationModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(paginationRequest.searchString))
            {
                paginationRequest.searchString.Trim();
                filters = Builders<StoreLocationModel>.Filter.Regex("Name", new MongoDB.Bson.BsonRegularExpression(paginationRequest.searchString, "i"))| Builders<StoreLocationModel>.Filter.Regex("Address", new MongoDB.Bson.BsonRegularExpression(paginationRequest.searchString, "i"));
            }                      
            return new
            {
                Message = "Get stores successfully",
                Data = await StoreCollection.Find(filters).ToListAsync(),               
            };
        }
    }
}
