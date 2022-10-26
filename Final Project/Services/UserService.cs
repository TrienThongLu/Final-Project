using Final_Project.Models;
using Final_Project.Requests.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Final_Project.Services
{
    public class UserService : IService<UserModel>
    {
        public readonly IMongoCollection<UserModel> userCollection;
        public readonly RoleService _roleService;
        public readonly StoreLocationService _storeService;

        public UserService(IConfiguration configuration, RoleService roleService, StoreLocationService storeService)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            userCollection = mongoClient.GetCollection<UserModel>("User");
            this._roleService = roleService;
            this._storeService = storeService;
        }

        public async Task<List<UserModel>> GetAsync()
        {
            var adminRole = await _roleService.RetrieveAdminRole();
            return await userCollection.Find(x => x.RoleId != adminRole.Id).ToListAsync();
        }

        public async Task<Object> GetAsync(UserPR paginationRequest)
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

            var _userData = userCollection.AsQueryable().Where(u => filters.Inject()).Skip((currentPage - 1) * perPage).Take(perPage);
            var _roleData = _roleService.roleCollection.AsQueryable();
            var _storeData = _storeService.StoreCollection.AsQueryable();

            var _result = from user in _userData
                          join role in _roleData
                          on user.RoleId equals role.Id
                          join store in _storeData
                          on user.StoreId equals store.Id into sList from store in sList.DefaultIfEmpty()
                          select new
                          {
                              Id = user.Id,
                              Fullname = user.Fullname,
                              PhoneNumber = user.PhoneNumber,
                              Gender = user.Gender,
                              DoB = user.DoB,
                              Ranking = user.Ranking,
                              IsBanned = user.IsBanned,
                              Role = new
                              {
                                  Id = role.Id,
                                  Name = role.Name
                              },
                              Store = new
                              {
                                  Id = user.StoreId != null ? store.Id : string.Empty,
                                  Name = user.StoreId != null ? store.Name : string.Empty
                              },
                          };

            return new
            {
                Message = "Get users successfully",
                Data = _result,
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

        public async Task<List<UserModel>> GetStoreCustomersAsync(string storeId)
        {
            var sCustomerRoleId = await _roleService.RetrieveStoreCustomerId();
            return await userCollection.Find(u => u.StoreId == storeId && u.RoleId == sCustomerRoleId).ToListAsync();
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

        public async Task<UserModel> GetCustomerViaPhonenumberAsync(string phonenumber)
        {
            var customerRoles = await _roleService.RetrieveOnlineCustomerRole();
            return await userCollection.Find(u => u.PhoneNumber == phonenumber && u.RoleId == customerRoles.Id).FirstOrDefaultAsync();
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
