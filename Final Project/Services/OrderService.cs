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
        private readonly StoreLocationService _storeService;

        public OrderService(IConfiguration configuration, UserService userService, StoreLocationService storeService)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString")).GetDatabase("FinalProject");
            orderCollection = mongoClient.GetCollection<OrderModel>("Order");
            this._userService = userService;
            this._storeService = storeService;
        }

        public async Task<List<OrderModel>> GetAsync()
        {
            return await orderCollection.Find(_ => true).ToListAsync(); ;
        }

        public async Task<OrderModel> GetAsync(string id)
        {
            return await orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Object> GetOrderAsync(string id)
        {
            var orderQuery = await orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
            var userQuery = new UserModel();
            if (orderQuery.TakenBy != null)
            {
                userQuery = await _userService.GetAsync(orderQuery.TakenBy);
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
                    name = orderQuery.TakenBy != null ? userQuery.Fullname : String.Empty
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
            return await orderCollection.Find(r => r.StoreId == storeId && r.Status == 4).SortByDescending(r => r.CreatedDate).ToListAsync();
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
            /*if (!string.IsNullOrEmpty(query.date))
            {
                query.searchString.Trim();
                filters = Builders<OrderModel>.Filter.Regex("Id", new MongoDB.Bson.BsonRegularExpression(query.searchString, "i"));
            }*/

            int currentPage = query.currentPage == 0 ? 1 : query.currentPage;
            int perPage = 10;
            decimal totalPage = Math.Ceiling((decimal)orderCollection.Find(filters).CountDocuments() / 10);

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

