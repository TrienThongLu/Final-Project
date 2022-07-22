using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public UserController(ILogger<UserController> logger, 
                              IConfiguration configuration, 
                              UserService userService, 
                              RoleService roleService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
        }
    }
}
