using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Utils.Helpers;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Final_Project.Requests.ItemTypeRequest;
using Final_Project.Requests.StoreLocation;
using Final_Project.Requests.Query;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreLocationController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ItemService _itemService;
        private readonly ItemTypeService _itemTypeService;
        private readonly ImageService _imageService;
        private readonly StoreLocationService _storeService;

        public StoreLocationController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              RoleService roleService,
                              ItemService itemService,
                              ItemTypeService itemTypeService,
                              ImageService imageService,
                              StoreLocationService storeService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._roleService = roleService;
            this._mappingService = mappingService;
            this._itemService = itemService;
            this._itemTypeService = itemTypeService;
            this._imageService = imageService;
            this._storeService = storeService;
        }

        /*[HttpGet("GetStore")]
        public async Task<IActionResult> getStoreList()
        {
            var _storeList = await _storeService.GetAsync();
            if (_storeList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Haven't store exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all store",
                Content = _storeList
            });
        }*/

        [HttpGet("GetStores")]
        public async Task<IActionResult> getListItems([FromQuery] StorePaginationRequest paginationRequest)
        {           
            return Ok(await _storeService.GetAsync(paginationRequest));
        }

        [HttpGet("GetStore/{id}")]
        public async Task<IActionResult> getStore(string id)
        {
            var _storeList = await _storeService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this store",
                Content = _storeList
            });
        }

        [HttpPost("AddStore")]
        public async Task<IActionResult> addStore([FromForm] AddStoreLocationRequest DataStore)
        {
            var _storeObject = _mappingService.Map<StoreLocationModel>(DataStore);
            await _storeService.CreateAsync(_storeObject);
            var _result = await _storeService.SearchTypeviaName(_storeObject.Name);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                });
            }            
            return Ok(new
            {
                Message = "Add Store successfully"
            });
        }

        [HttpDelete("DeleteStore/{id}")]
        public async Task<IActionResult> deleteStore(string id)
        {          
                if (await _storeService.GetAsync(id) == null) return NotFound();              
                {
                    await _storeService.DeleteAsync(id);
                }
                return Ok(new
                {
                    Message = "This Store has been deleted"
                });           
        }

        [HttpPut("UpdateStore")]
        public async Task<IActionResult> updateStore([FromForm] UpdateStoreLocaionRequest updateInfo)
        {
            var updateStore = await _storeService.GetAsync(updateInfo.Id);
            if (updateStore == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Store not exist"
                });
            }
            updateStore = _mappingService.Map<UpdateStoreLocaionRequest, StoreLocationModel>(updateInfo, updateStore);           
            await _storeService.UpdateAsync(updateInfo.Id, updateStore);            
            return Ok(new
            {
                Message = "Update Store successfully"
            });
        }
    }
}

