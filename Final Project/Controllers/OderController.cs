using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.OderRequests;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OderController:ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly ItemService _itemService;
        private readonly ImageService _imageService;
        private readonly RoleService _roleService;
        private readonly ItemTypeService _itemTypeService;
        private readonly OderService _oderService;
        public OderController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              ItemService itemService,
                              ImageService imageService,
                              RoleService roleService,
                              ItemTypeService itemTypeService,
                              OderService oderService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._itemService = itemService;
            this._mappingService = mappingService;
            this._imageService = imageService;
            this._roleService = roleService;
            this._itemTypeService = itemTypeService;
            this._oderService = oderService;
        }
        [HttpGet("GetOder")]
        public async Task<IActionResult> getOderList()
        {
            var _odersList = await _oderService.GetAsync();
            if (_odersList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No oder exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all items",
                Content = _odersList
            });
        }

        [HttpGet("GetOder/{id}")]
        public async Task<IActionResult> getOder(string id)
        {
            var _odersList = await _oderService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this Oder",
                Content = _odersList
            });
        }

        [HttpPost("CreateOder")]
        public async Task<IActionResult> createOder([FromForm] CreateOderRequest newOder)
        {
            var _oderObject = _mappingService.Map<OderModel>(newOder);
            _oderObject.CreatedDate=DateTime.Now;
            _oderObject.UpdatedDate=DateTime.Now;
            await _oderService.CreateAsync(_oderObject);
            var _result = await _itemService.GetAsync(_oderObject.Id);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create Oder"
                });
            }
            return Ok(new
            {
                Message = "Create oder successfully"
            });
        }
        [HttpDelete("DeleteItem/{id}")]
        public async Task<IActionResult> deleteItem(string id)
        {
            if (await _itemService.GetAsync(id) == null) return NotFound();
            await _itemService.DeleteAsync(id);
            await _imageService.deleteImage(id);
            return Ok(new
            {
                Message = "Item has been deleted"
            });
        }

        [HttpPut("UpdateOder")]
        public async Task<IActionResult> updateOder([FromForm] UpdateOderRequest updateInfo)
        {
            var updateOder = await _oderService.GetAsync(updateInfo.OderId);
            if (updateOder == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Oder not exist"
                });
            }
            updateOder = _mappingService.Map<UpdateOderRequest, OderModel>(updateInfo, updateOder);
            await _oderService.UpdateAsync(updateInfo.OderId, updateOder);
            return Ok(new
            {
                Message = "Update Item successfully"
            });
        }
    }
}
