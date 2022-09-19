using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Final_Project.Requests.ItemTypeRequest;
using Final_Project.Requests.GroupSize;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "Admin")]
    public class GroupSizeController:ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ItemService _itemService;
        private readonly ItemTypeService _itemTypeService;
        private readonly ImageService _imageService;
        private readonly GroupSizeService _sizeService;

        public GroupSizeController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              RoleService roleService,
                              ItemService itemService,
                              ItemTypeService itemTypeService,
                              ImageService imageService,
                              GroupSizeService sizeService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._mappingService = mappingService;
            this._itemService = itemService;
            this._itemTypeService = itemTypeService;
            this._imageService = imageService;
            this._sizeService = sizeService;
        }

        [HttpGet("GetSize")]
        public async Task<IActionResult> getSizeList()
        {
            var _sizeList = await _sizeService.GetAsync();
            if (_sizeList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Haven't size exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all sizes",
                Content = _sizeList
            });
        }

        [HttpGet("GetType/{id}")]
        public async Task<IActionResult> getSizebyId(string id)
        {
            var _sizeList = await _sizeService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this size",
                Content = _sizeList
            });
        }

        [HttpPost("AddSize")]
        public async Task<IActionResult> addSize([FromBody] GroupSizeRequest Input)
        {
            var _sizeObject = _mappingService.Map<GroupSizeModel>(Input);
            await _sizeService.CreateAsync(_sizeObject);
            var _result = await _sizeService.SearchTypeviaName(_sizeObject.Size);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                });
            }           
            return Ok(new
            {
                Message = "Create successfully",
                Content = _result
            });
        }

        [HttpDelete("DeleteSize/{id}")]
        public async Task<IActionResult> deleteSize(string id)
        {
            try
            {               
                if (await _sizeService.GetAsync(id) == null) return NotFound();
                await _sizeService.DeleteAsync(id);
                return Ok(new
                {
                    Message = "This Size has been deleted"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot delete this Size"
                });
            }
        }
    }
}
