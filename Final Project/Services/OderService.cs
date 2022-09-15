using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{  
    public class OderService : IService<OderModel>
    {
        public readonly IMongoCollection<OderModel> oderCollection;
        public OderService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            oderCollection = mongoClient.GetCollection<OderModel>("Oder");
        }

        public async Task<List<OderModel>> GetAsync()
        {
            return await oderCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<OderModel> GetAsync(string id)
        {
            return await oderCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(OderModel objectData)
        {
            await oderCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await oderCollection.DeleteOneAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(string id, OderModel objectData)
        {
            await oderCollection.ReplaceOneAsync(r => r.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }       
    }
}

