using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Final_Project.Requests.ItemTypeRequest;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "Admin")]
    public class ItemTypeController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ItemService _itemService;
        private readonly ItemTypeService _itemTypeService;
        private readonly ImageService _imageService;

        public ItemTypeController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              RoleService roleService,
                              ItemService itemService,
                              ItemTypeService itemTypeService,
                              ImageService imageService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._mappingService = mappingService;
            this._itemService = itemService;
            this._itemTypeService = itemTypeService;
            this._imageService = imageService;
        }

        [HttpGet("GetType")]
        public async Task<IActionResult> getTypeList()
        {
            var _typeList = await _itemTypeService.GetAsync();
            if (_typeList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "haven't type exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all types",
                Content = _typeList
            });
        }

        [HttpGet("GetType/{id}")]
        public async Task<IActionResult> getType(string id)
        {
            var _typeList = await _itemTypeService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this type",
                Content = _typeList
            });
        }

        [HttpPost("AddType")]
        public async Task<IActionResult> addType([FromBody] AddItemTypeRequest InputType)
        {
            var _typeObject = _mappingService.Map<ItemTypeModel>(InputType);
            await _itemTypeService.CreateAsync(_typeObject);

            var _result = await _itemTypeService.SearchTypeviaName(_typeObject.Name);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create type, missing date input"
                });
            }
            await _imageService.uploadImage(_result.Id, InputType.Image);
            return Ok(new
            {
                Message = "Create type successfully",
                Content = _result
            });
        }

        [HttpDelete("DeleteType{id}")]
        public async Task<IActionResult> deleteType(string id)
        {
            if (await _itemTypeService.GetAsync(id) == null) return NotFound();
           
            await _itemTypeService.DeleteAsync(id);
            await _imageService.deleteImage(id);
            return Ok(new
            {
                Message = "This Type has been deleted"
            });
        }
    }
}
