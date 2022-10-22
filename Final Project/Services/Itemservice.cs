using Final_Project.Models;
using Final_Project.Requests.Query;
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
            return await itemCollection.Find(_ => true).ToListAsync(); 
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

        public async Task<ItemModel> SearchItemviaName(string Name)
        {
            return await itemCollection.Find(r => r.Name == Name).FirstOrDefaultAsync();
        }
        public async Task<bool> ItemIsUsed(string id)
        {
            return await itemCollection.Find(t => t.TypeId == id).AnyAsync();
        }        
        public async Task <List<ItemModel>> GetlistItembytype(string id)
        {
           return itemCollection.Find(t => t.TypeId == id).ToList();
        }

        public async Task<Object> GetAsync(ItemPR paginationRequest)
        {
            var filters = Builders<ItemModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(paginationRequest.searchString))
            {
                paginationRequest.searchString.Trim();
                filters = Builders<ItemModel>.Filter.Regex("Name", new MongoDB.Bson.BsonRegularExpression(paginationRequest.searchString, "i"));
            }
            if (!string.IsNullOrEmpty(paginationRequest.typeId))
            {
                paginationRequest.typeId.Trim();
                filters = Builders<ItemModel>.Filter.Eq(i=>i.TypeId, paginationRequest.typeId);
            }
            int currentPage = paginationRequest.currentPage == 0 ? 1 : paginationRequest.currentPage;
            int perPage = 10;
            decimal totalPage = Math.Ceiling((decimal)itemCollection.Find(filters).CountDocuments() / 10);
            return new
            {
                Message = "Get items successfully",
                Data = await itemCollection.Find(filters).Skip((currentPage - 1) * perPage).Limit(perPage).ToListAsync(),
                CurrentPage = currentPage,
                TotalPage = totalPage,
            };
        }
    }
}
