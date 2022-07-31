using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Final_Project.Requests.UserRequests;
using Final_Project.Utils.Services;
using AutoMapper;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    /*[Authorize]*/
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly TokenService _tokenService;
        private readonly SendSMSService _smsService;
        private readonly OTPService _otpService;

        public UserController(ILogger<UserController> logger, 
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              RoleService roleService,
                              TokenService tokenService,
                              SendSMSService smsService,
                              OTPService otpService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._tokenService = tokenService;
            this._mappingService = mappingService;
            this._smsService = smsService;
            this._otpService = otpService;
        }

        [HttpGet("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> login([FromBody] LoginRequest form)
        {
            var _userData = await _userService.LoginAsync(form.PhoneNumber);
            if (_userData == null || !(HMACSHA512Helper.VerifyPasswordHash(form.Password, _userData.PasswordHash, _userData.PasswordSalt)))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Wrong Password",
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

        [HttpGet("CheckPhonenumber/{phonenumber}")]
        public async Task<IActionResult> checkPhonenumber(string phonenumber)
        {
            if (phonenumber.Length < 10 || phonenumber.Length > 12)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Please enter valid phonenumber"
                });
            }

            var _userData = await _userService.LoginAsync(phonenumber);
            if (_userData == null)
            {
                return BadRequest(new
                {
                    Error = "Unavailable",
                    Message = "This phonenumber need to be register",
                });
            }

            return Ok(new
            {
                Message = "This phonenumber is already taken"
            });
        }

        [HttpPost("RegisterRequest/{phonenumber}")]
        public async Task<IActionResult> registerRequest(string phonenumber)
        {
            if (phonenumber.Length is < 10 or > 12)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Please enter valid phonenumber"
                });
            }

            var _userData = await _userService.LoginAsync(phonenumber);
            if (_userData != null)
            {
                return BadRequest(new
                {
                    Error = "Unavailable",
                    Message = "This phonenumber is already taken",
                });
            }

            OTPModel _otpObject = new OTPModel() { PhoneNumber = phonenumber, Type = "Register" };

            string _otpCode = await _otpService.generateOTP(_otpObject);

            string message = $"Please enter this otp code: {_otpCode} to register.";

            bool _sendSms = await _smsService.SendSMS(phonenumber, message);

            if (!_sendSms)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot send sms"
                });
            }

            return Ok(new
            {
                Message = "Register request created successfully",
            });
        }

        [HttpPost("ReceiveOTP/{otp}")]
        public async Task<IActionResult> receiveOTP([FromBody] RegisterRequest newUserData, string otp)
        {
            OTPModel _otpObject = await _otpService.getOTP(otp, newUserData.PhoneNumber);
            if (_otpObject == null || _otpObject.ExpireAt < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "OTP expired"
                });
            }

            var _DefaultUserRole = await _roleService.RetrieveUserRole();
            HMACSHA512Helper.CreatePasswordHash(newUserData.Password, out byte[] passwordHash, out byte[] passwordSalt);

            UserModel _userObject = new UserModel
            {
                Fullname = newUserData.FullName,
                PhoneNumber = _otpObject.PhoneNumber,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                RoleId = _DefaultUserRole.Id,
            };
            await _userService.CreateAsync(_userObject);
            var _result = await _userService.LoginAsync(_userObject.PhoneNumber);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot register"
                });
            }
            return Ok(new
            {
                Message = "Register successfully",
                Content = _result
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
        [Authorize]
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
        public async Task<IActionResult> createUser([FromBody] CreateRequest newUserData)
        {
            var _userObject = _mappingService.Map<UserModel>(newUserData);
            var _adminRoleId = (await _roleService.RetrieveAdminRole()).Id;

            if (String.Equals(_userObject.RoleId, _adminRoleId) && await _userService.AlreadyHasAdmin())
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Creation Fail"
                });
            }

            HMACSHA512Helper.CreatePasswordHash("chethaiyphuong", out byte[] passwordHash, out byte[] passwordSalt);
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
