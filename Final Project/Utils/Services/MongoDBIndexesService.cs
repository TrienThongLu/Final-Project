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
        private readonly IMongoClient _mongoClient;

        public MongoDBIndexesService(ILogger<MongoDBIndexesService> logger,
                       IConfiguration configuration,
                       UserService userService,
                       RoleService roleService)
        {
            _mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString"));
            this._logger = logger;
            this._userService = userService;
            this._roleService = roleService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UserModel.UniqueUsernameIndex(_userService, _logger);
                await RoleModel.UniqueRoleIndex(_roleService, _logger);
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
