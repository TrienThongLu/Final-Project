using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using Final_Project.Requests.RoleRequests;
using AutoMapper;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RoleController : ControllerBase
    {      
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;      

        public RoleController(ILogger<UserController> logger, 
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              RoleService roleService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._mappingService = mappingService;
        }

        [HttpGet("GetRole")]
        public async Task<IActionResult> getRoleList()
        {
            var _rolesList = await _roleService.GetAsync();
            if (_rolesList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No roles exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all roles",
                Content = _rolesList
            });
        }

        [HttpGet("GetRole/{id}")]
        public async Task<IActionResult> getRole(string id)
        {
            var _roleList = await _roleService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get all roles",
                Content = _roleList
            });
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> addRole([FromBody] AddRoleRequest newRoleData)
        {
            var _roleObject = _mappingService.Map<RoleModel>(newRoleData);
            await _roleService.CreateAsync(_roleObject);

            var _result = await _roleService.SearchRoleviaName(_roleObject.Name);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create role"
                });
            }
            return Ok(new
            {
                Message = "Create role successfully",
                Content = _result
            });
        }

        [HttpDelete("DeleteRole/{id}")]
        public async Task<IActionResult> deleteRole(string id)
        {
            if (await _roleService.GetAsync(id) == null) return NotFound();
            if (await _userService.RoleIsUsed(id))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Role is in used"
                });
            }
            await _roleService.DeleteAsync(id);

            return Ok(new
            {
                Message = "Role has been deleted"
            });
        }
    }
}
