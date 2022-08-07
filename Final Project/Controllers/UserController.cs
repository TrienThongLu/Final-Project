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

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> login([FromBody] LoginRequest form)
        {
            var _userData = await _userService.GetViaPhonenumberAsync(form.PhoneNumber);
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

            var _userData = await _userService.GetViaPhonenumberAsync(phonenumber);
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

        [HttpPost("sendOTP/{phonenumber},{type}")]
        public async Task<IActionResult> sendOTP(string phonenumber, string type)
        {
            if ((phonenumber.Length is < 10 or > 12) || (type != "Register" && type != "ResetPassword"))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Please enter valid phonenumber"
                });
            }

            var _userData = await _userService.GetViaPhonenumberAsync(phonenumber);
            if (_userData != null && type == "Register")
            {
                return BadRequest(new
                {
                    Error = "Unavailable",
                    Message = "This phonenumber is already taken",
                });
            }

            OTPModel _otpObject = new OTPModel() { PhoneNumber = phonenumber, Type = type };

            string _otpCode = await _otpService.generateOTP(_otpObject);

            string message = $"Please enter this otp code: {_otpCode} to {type}.";

            /*bool _sendSms = await _smsService.SendSMS(phonenumber, message);

            if (!_sendSms)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot send sms"
                });
            }*/

            return Ok(new
            {
                Message = "Register request created successfully",
                OTP = _otpCode,
            });
        }

        [HttpPost("Register/{otp}")]
        public async Task<IActionResult> register([FromBody] RegisterRequest newUserData, string otp)
        {
            OTPModel _otpObject = await _otpService.getOTP(otp, newUserData.PhoneNumber);
            if (_otpObject == null || _otpObject.ExpireAt < DateTime.UtcNow || _otpObject.Type != "Register")
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "OTP expired or OTP Error"
                });
            }

            var _DefaultUserRole = await _roleService.RetrieveUserRole();
            HMACSHA512Helper.CreatePasswordHash(newUserData.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            var _userObject = _mappingService.Map<UserModel>(newUserData);
            _userObject.PasswordHash = passwordHash;
            _userObject.PasswordSalt = passwordSalt;
            _userObject.RoleId = _DefaultUserRole.Id;
            _userObject.Ranking = "Silver";

            await _userService.CreateAsync(_userObject);
            var _result = await _userService.GetViaPhonenumberAsync(_userObject.PhoneNumber);
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

        [HttpPost("ResetPassword/{otp}")]
        public async Task<IActionResult> resetPassword([FromBody] ResetPasswordRequest userData, string otp)
        {
            OTPModel _otpObject = await _otpService.getOTP(otp, userData.PhoneNumber);
            if (_otpObject == null || _otpObject.ExpireAt < DateTime.UtcNow || _otpObject.Type != "ResetPassword")
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "OTP expired"
                });
            }
            HMACSHA512Helper.CreatePasswordHash(userData.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var _userObject = await _userService.GetViaPhonenumberAsync(userData.PhoneNumber);
            if (_userObject == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "User not exist"
                });
            }
            _userObject.PasswordHash = passwordHash;
            _userObject.PasswordSalt = passwordSalt;

            await _userService.UpdateAsync(_userObject.Id, _userObject);

            return Ok(new
            {
                Message = "Reset password successfully",
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
            _userObject.Ranking = "Silver";
            await _userService.CreateAsync(_userObject);
            var _result = await _userService.GetViaPhonenumberAsync(_userObject.PhoneNumber);
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

        [HttpPut("UpdateProfileUser")]
        public async Task<IActionResult> updateProfileUser([FromBody] UpdateProfileRequest updateInfo)
        {
            var UserToUpdate = await _userService.GetAsync(updateInfo.Id);
            if (UserToUpdate == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "User not exist"
                });
            }
            UserToUpdate = _mappingService.Map<UpdateProfileRequest, UserModel > (updateInfo, UserToUpdate);
            await _userService.UpdateAsync(updateInfo.Id, UserToUpdate);
            return Ok(new
            {
                Message = "Update user successfully"
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
