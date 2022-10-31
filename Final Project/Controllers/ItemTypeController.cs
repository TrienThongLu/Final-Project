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
    [Authorize(Roles = "Admin")]
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
        [AllowAnonymous]
        public async Task<IActionResult> getTypeList()
        {
            var _typeList = await _itemTypeService.GetAsync();
            if (_typeList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Haven't type exist"
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
        public async Task<IActionResult> addType([FromForm] AddItemTypeRequest InputType)
        {
            var _typeObject = _mappingService.Map<ItemTypeModel>(InputType);
            await _itemTypeService.CreateAsync(_typeObject);
            var _result = await _itemTypeService.SearchTypeviaName(_typeObject.Name);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                });
            }
            await _imageService.uploadTypeImage(_result.Id, InputType.Image);
            return Ok(new
            {
                Message = "Create type successfully"
            });
        }

        [HttpDelete("DeleteType/{id}")]
        public async Task<IActionResult> deleteType(string id)
        {
            try
            {
                if (await _itemService.ItemIsUsed(id)) return BadRequest(new { Message = "This type is used" }); ;
                if (await _itemTypeService.GetAsync(id) == null) return NotFound();           
                if(await _imageService.deleteTypeImage(id))
                {
                    await _itemTypeService.DeleteAsync(id);
                }
                return Ok(new
                {
                    Message = "This Type has been deleted"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot delete Item Type"
                });
            }
        }

        [HttpPut("UpdateType")]
        public async Task<IActionResult> updateType([FromForm] UpdateTypeRequest updateInfo)
        {
            var updateType = await _itemTypeService.GetAsync(updateInfo.TypeId);
            if (updateType == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Type not exist"
                });
            }
            updateType = _mappingService.Map<UpdateTypeRequest, ItemTypeModel>(updateInfo, updateType);
            if (!(updateInfo.ImageUpload is string) && !(updateInfo.ImageUpload is null))
            {
                await _imageService.deleteTypeImage(updateInfo.TypeId);
            }
            await _itemTypeService.UpdateAsync(updateInfo.TypeId, updateType);
            if (!(updateInfo.ImageUpload is string) && !(updateInfo.ImageUpload is null))
            {
                await _imageService.uploadTypeImage(updateInfo.TypeId, updateInfo.ImageUpload);
            }

            return Ok(new
            {
                Message = "Update Item successfully"
            });
        }
    }
}
