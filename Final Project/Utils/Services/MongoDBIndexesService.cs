using AutoMapper;
using Final_Project.Models;
using Final_Project.Services;
using MongoDB.Driver;

namespace Final_Project.Utils.Services
{
    public class MongoDBIndexesService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ItemService _itemService;
        private readonly ItemTypeService _itemTypeService;
        private readonly ToppingService _toppingService;
        private readonly OTPService _otpService;
        private readonly IMongoClient _mongoClient;

        public MongoDBIndexesService(ILogger<MongoDBIndexesService> logger,
                       IConfiguration configuration,
                       UserService userService,
                       RoleService roleService,
                       ItemService itemService,
                       ItemTypeService itemTypeService,
                       ToppingService toppingService,
                       OTPService otpService)
        {
            _mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString"));
            this._logger = logger;
            this._userService = userService;
            this._roleService = roleService;
            this._otpService = otpService;
            this._itemService = itemService;
            this._itemTypeService = itemTypeService;
            this._toppingService = toppingService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UserModel.UniqueUsernameIndex(_userService, _logger);
                await RoleModel.UniqueRoleIndex(_roleService, _logger);
                await ItemModel.UniqueItemIndex(_itemService, _logger);
                await ItemTypeModel.UniqueItemTypeIndex(_itemTypeService, _logger);
                await ToppingModel.UniqueToppingIndex(_toppingService, _logger);
                await OTPModel.ExpireAtTimerIndex(_otpService, _logger);
                _logger.LogInformation("Success");
            }
            catch
            {
                _logger.LogError("Error");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
