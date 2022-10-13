using Final_Project.Models;
using Final_Project.Requests.Query;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class UserService : IService<UserModel>
    {
        public readonly IMongoCollection<UserModel> userCollection;
        public readonly RoleService _roleService;

        public UserService(IConfiguration configuration, RoleService roleService)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            userCollection = mongoClient.GetCollection<UserModel>("User");
            this._roleService = roleService;
        }

        public async Task<List<UserModel>> GetAsync()
        {
            var adminRole = await _roleService.RetrieveAdminRole();
            return await userCollection.Find(x => x.RoleId != adminRole.Id).ToListAsync();
        }

        public async Task<Object> GetAsync(UserPaginationRequest paginationRequest)
        {
            var adminRole = await _roleService.RetrieveAdminRole();
            var filters = !Builders<UserModel>.Filter.Eq(u => u.RoleId, adminRole.Id);
            if (!string.IsNullOrEmpty(paginationRequest.role))
            {
                filters &= Builders<UserModel>.Filter.Eq(u => u.RoleId, paginationRequest.role);
            }
            if (!string.IsNullOrEmpty(paginationRequest.ranking))
            {
                filters &= Builders<UserModel>.Filter.Eq(u => u.Ranking, paginationRequest.ranking);
            }
            if (!string.IsNullOrEmpty(paginationRequest.searchString))
            {
                paginationRequest.searchString.Trim();
                filters &= Builders<UserModel>.Filter.Regex("PhoneNumber", new MongoDB.Bson.BsonRegularExpression(paginationRequest.searchString)) | Builders<UserModel>.Filter.Regex("Fullname", new MongoDB.Bson.BsonRegularExpression(paginationRequest.searchString, "i"));
            }
            int currentPage = paginationRequest.currentPage == 0 ? 1 : paginationRequest.currentPage;
            int perPage = 10;
            decimal totalPage = Math.Ceiling((decimal)userCollection.Find(filters).CountDocuments() / 10);

            return new
            {
                Message = "Get users successfully",
                Data = await userCollection.Find(filters).Skip((currentPage - 1) * perPage).Limit(perPage).ToListAsync(),
                CurrentPage = currentPage,
                TotalPage = totalPage,
            };
        }

        public async Task<UserModel> GetAsync(string id)
        {
            return await userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<UserModel>> SearchAsync(string searchString)
        {
            var adminRole = await _roleService.RetrieveAdminRole();
            var filters = !Builders<UserModel>.Filter.Eq(u => u.RoleId , adminRole.Id);
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString.Trim();
                filters &= Builders<UserModel>.Filter.Regex("PhoneNumber", new MongoDB.Bson.BsonRegularExpression(searchString)) | Builders<UserModel>.Filter.Regex("Fullname", new MongoDB.Bson.BsonRegularExpression(searchString, "i"));
            }
            return await userCollection.Find(filters).ToListAsync();
        }

        public async Task CreateAsync(UserModel objectData)
        {
            await userCollection.InsertOneAsync(objectData);
        }

        public async Task DeleteAsync(string id)
        {
            await userCollection.DeleteOneAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(string id, UserModel objectData)
        {
            await userCollection.ReplaceOneAsync(u => u.Id == id, objectData, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<UserModel> GetViaPhonenumberAsync(string phonenumber)
        {
            return await userCollection.Find(u => u.PhoneNumber == phonenumber).FirstOrDefaultAsync();
        }

        public async Task<bool> AlreadyHasAdmin()
        {
            var _idRoleAdmin = await _roleService.RetrieveAdminRole();
            return await userCollection.Find(u => u.RoleId == _idRoleAdmin.Id).AnyAsync();
        }

        public async Task<bool> RoleIsUsed(string id)
        {
            return await userCollection.Find(u => u.RoleId == id).AnyAsync();
        }

        public async Task AddNewAddress(string id, string address)
        {
            var _userObject = await userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (_userObject.Addresses == null)
            {
                _userObject.Addresses = new List<string>();
            }
            _userObject.Addresses.Add(address);

            await userCollection.ReplaceOneAsync(x => x.Id == id, _userObject);
        }

        public async Task RemoveAddress(string id, string address)
        {
            var _userObject = await userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            _userObject.Addresses.Remove(address);

            await userCollection.ReplaceOneAsync(x => x.Id == id, _userObject);
        }
    }
}
