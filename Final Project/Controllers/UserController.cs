using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        #region Form

        public record LoginForm(long phonenumber, string password);
        public record UserCreationForm(string fullname, int phonenumber, string RoleId);
        public record UserRegisterationForm(string fullname, string phonenumber, string password);

        #endregion

        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly TokenService _tokenService;

        public UserController(ILogger<UserController> logger, 
                              IConfiguration configuration,
                              UserService userService,
                              RoleService roleService,
                              TokenService tokenService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._tokenService = tokenService;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> login([FromBody] LoginForm form)
        {
            var _userData = await _userService.LoginAsync(form.phonenumber);
            if (_userData == null || !(HMACSHA512Helper.VerifyPasswordHash(form.password, _userData.PasswordHash, _userData.PasswordSalt)))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Wrong Username or Password",
                });
            }
            if (_userData.IsBanned == true)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "This account has been suspended!"
                });
            }
            var _roleData = await _roleService.GetAsync(_userData.RoleId);
            TokenModel tokenModel = _tokenService.GenerateJwt(_userData, _roleData.Name);

            var _result = new
            {
                Id = _userData.Id,
                Fullname = _userData.Fullname,
                Phonenumber = _userData.PhoneNumber,
                RoleName = _roleData.Name,
            };

            await _tokenService.CreateAsync(tokenModel);
            return Ok(new
            {
                Message = "Login successfully",
                Content = new
                {
                    Token = tokenModel,
                    User = _result
                }
            });
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> getListUser()
        {
            var _usersList = await _userService.GetAsync();
            if (_usersList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No user exist"
                });
            }
            return Ok(new
            {
                Message = $"Successfully get users",
                Content = _usersList
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

            HMACSHA512Helper.CreatePasswordHash("123123", out byte[] passwordHash, out byte[] passwordSalt);
            _userObject.PasswordHash = passwordHash;
            _userObject.PasswordSalt = passwordSalt;
            await _userService.CreateAsync(_userObject);
            var _result = await _userService.LoginAsync(_userObject.PhoneNumber);
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

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (await _userService.GetAsync(id) == null) return NotFound();
            var _AdminRole = await _roleService.RetrieveAdminRole();
            var _userData = await _userService.GetAsync(id);
            if (_userData.Id == _AdminRole.Id)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot delete admin"
                });
            }
            await _userService.DeleteAsync(id);

            return Ok(new
            {
                Message = "User has been deleted"
            });
        }
    }
}
