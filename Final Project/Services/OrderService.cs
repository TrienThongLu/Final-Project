using Final_Project.Models;
using Final_Project.Requests.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Final_Project.Services
{  
    public class OrderService : IService<OrderModel>
    {
        public readonly IMongoCollection<OrderModel> orderCollection;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly StoreLocationService _storeService;

        public OrderService(IConfiguration configuration, UserService userService, RoleService roleService, StoreLocationService storeService)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            orderCollection = mongoClient.GetCollection<OrderModel>("Order");
            this._userService = userService;
            this._roleService = roleService;
            this._storeService = storeService;
        }

        public async Task<List<OrderModel>> GetAsync()
        {
            return await orderCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Object> GetAsync(AdminGetOrdersPR query)
        {
            var filters = Builders<OrderModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(query.storeId))
            {
                filters = Builders<OrderModel>.Filter.Where(o => o.StoreId == query.storeId);
            }
            if (!string.IsNullOrEmpty(query.type))
            {
                filters = Builders<OrderModel>.Filter.Where(o => o.Type == Int32.Parse(query.type));
            }
            if (!string.IsNullOrEmpty(query.date))
            {
                long date = long.Parse(query.date);
                long dateP = date + 86400000;
                filters = Builders<OrderModel>.Filter.Where(o => o.CreatedDate >= date && o.CreatedDate <= dateP);
            }

            int currentPage = query.currentPage == 0 ? 1 : query.currentPage;
            int perPage = 10;
            decimal totalPage = Math.Ceiling((decimal)orderCollection.Find(o => filters.Inject()).CountDocuments() / 10);

            var _orderData = orderCollection.AsQueryable().Where(o => filters.Inject()).OrderByDescending(o => o.CreatedDate).Skip((currentPage - 1) * perPage).Take(perPage);
            var _storeData = _storeService.StoreCollection.AsQueryable();

            var _result = from o in _orderData
                          join s in _storeData
                          on o.StoreId equals s.Id
                          select new
                          {
                              Id = o.Id,
                              Type = o.Type == 1 ? "At Store" : "Online",
                              Store = s.Name,
                              Status = o.Status == 0 ? "Pending Payment" : (o.Status == 1 ? "Processing" : (o.Status == 2 ? "On Delivery" : (o.Status == 3 ? "Completed" : "Failed"))),
                              Amount = o.Amount,
                              CreatedDate = o.CreatedDate,
                              PaymentMethod = o.PaymentMethod,
                          };

            return new
            {
                Message = "Get orders successfully",
                Data = _result,
                CurrentPage = currentPage,
                TotalPage = totalPage,
            };
        }

        public async Task<OrderModel> GetAsync(string id)
        {
            return await orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Object> GetOrderAsync(string id)
        {
            var orderQuery = await orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
            var userQuery = new UserModel();
            bool isCustomer = false;
            if (orderQuery.TakenBy != null)
            {
                userQuery = await _userService.GetAsync(orderQuery.TakenBy);
                string customerRoleId = await _roleService.RetrieveStoreCustomerId();
                if (userQuery.RoleId == customerRoleId)
                {
                    isCustomer = true;
                }
            }

            var orderData = new
            {
                orderQuery.Id,
                orderQuery.Status,
                orderQuery.Type,
                orderQuery.TotalPrice,
                orderQuery.DiscountPercent,
                orderQuery.Amount,
                orderQuery.IsDone,
                orderQuery.IsPaid,
                orderQuery.CustomerInfo,
                orderQuery.PaymentInfo,
                orderQuery.PaymentMethod,
                orderQuery.StoreId,
                orderQuery.Items,
                orderQuery.CreatedDate,
                orderQuery.Note,
                takenBy = new
                {
                    id = orderQuery.TakenBy != null ? orderQuery.TakenBy : String.Empty,
                    name = orderQuery.TakenBy != null ? userQuery.Fullname : String.Empty,
                    isCustomer = isCustomer
                }
            };


            return orderData;
        }

        public async Task<List<OrderModel>> GetTop5CompletedOrdersAsync(string storeId)
        {
            return await orderCollection.Find(r => r.StoreId == storeId && r.Status == 3).SortByDescending(r => r.CreatedDate).Limit(5).ToListAsync();
        }

        public async Task<List<OrderModel>> GetUnpaidOrdersAsync(string storeId)
        {
            return await orderCollection.Find(r => r.StoreId == storeId && r.Type == 1 && r.Status == 0 && !r.IsPaid).SortByDescending(r => r.CreatedDate).ToListAsync();
        }
        public async Task<List<OrderModel>> GetCompletedOrdersAsync(string storeId)
        {
            return await orderCollection.Find(r => r.StoreId == storeId && r.Status == 3).SortByDescending(r => r.CreatedDate).ToListAsync();
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

        public async Task<Object> UserGetOrdersAsync(UserGetOrdersPR query, string id)
        {
            var filters = Builders<OrderModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(query.status))
            {
                query.status.Trim();
                filters = Builders<OrderModel>.Filter.Where(o => o.Status == Int32.Parse(query.status));
            }

            int currentPage = query.currentPage == 0 ? 1 : query.currentPage;
            int perPage = 10;
            decimal totalPage = Math.Ceiling((decimal)orderCollection.Find(o => o.CustomerInfo != null && o.CustomerInfo.Id == id && filters.Inject()).CountDocuments() / 10);

            var _orderData = orderCollection.AsQueryable().Where(o => o.CustomerInfo != null && o.CustomerInfo.Id == id && filters.Inject()).OrderByDescending(o => o.CreatedDate).Skip((currentPage - 1) * perPage).Take(perPage);
            var _storeData = _storeService.StoreCollection.AsQueryable();

            var _result = from o in _orderData
                          join s in _storeData
                          on o.StoreId equals s.Id
                          select new
                          {
                              Id = o.Id,
                              Type = o.Type == 1 ? "At Store" : "Online",
                              Store = s.Name,
                              Status = o.Status == 0 ? "Pending Payment" : (o.Status == 1 ? "Processing" : (o.Status == 2 ? "On Delivery" : (o.Status == 3 ? "Completed" : "Failed"))),
                              Amount = o.Amount,
                              CreatedDate = o.CreatedDate,
                              PaymentMethod = o.PaymentMethod,
                          };

            return new
            {
                Message = "Get orders successfully",
                Data = _result,
                CurrentPage = currentPage,
                TotalPage = totalPage,
            };
        }
    }
}

