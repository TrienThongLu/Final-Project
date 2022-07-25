using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        #region Form

        public record LoginForm(string username, string password);
        public record UserCreationForm(string fullname, int phonenumber, string RoleId);
        public record UserRegisterationForm(string fullname, string phonenumber, string password);

        #endregion

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

        [HttpGet("GetUser")]
        public async Task<IActionResult> getListUser()
        {
            var _users = await _userService.GetAsync();
            if (_users == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No users exist"
                });
            }
            return Ok(new
            {
                Message = $"Successfully get users",
                Content = _users
            });
        }

        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> getUser(string id)
        {
            var _userData = await _userService.GetAsync(id);
            if (_userData == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "User not exist"
                });
            }
            var _roleData = await _roleService.GetAsync(_userData.RoleId);
            var _result = new
            {
                Id = _userData.Id,
                Fullname = _userData.Fullname,
                Age = _userData.Age,
                Gender = _userData.Gender,
                PhoneNumber = _userData.PhoneNumber,
                Addresses = _userData.Addresses,
                DoB = _userData.DoB,
                Role = _roleData.Name,
                Banned = _userData.IsBanned,
                Ranking = _userData.Ranking,
                Point = _userData.Point
            };
            return Ok(new
            {
                Message = $"Successfully get user: {_result.Fullname}",
                Content = _result
            });
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> createUser([FromBody]UserCreationForm newUserData)
        {
            var _userObject = ModelConvertHelper.Convert<UserModel>(newUserData);
            var _adminRoleId = (await _roleService.RetrieveAdminRole()).Id;

            if (String.Equals(_userObject.RoleId, _adminRoleId) && await _userService.AlreadyHasAdmin())
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Creation Fail"
                });
            }

            _userObject.Password = Sh256HashHelper.Sh256Hash("123123");
            await _userService.CreateAsync(_userObject);
            var _result = await _userService.LoginAsync(_userObject.PhoneNumber, _userObject.Password);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create account"
                });
            }
            return Ok(new
            {
                Message = "Create account successfully",
                Content = _result
            });
        }

        /*[HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteHero(string id)
        {
            if (await _roleService.GetAsync(id) == null) return NotFound();
        }*/
    }
}
