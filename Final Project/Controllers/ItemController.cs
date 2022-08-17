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
        public class ItemController : ControllerBase
        {  
            private readonly ILogger<UserController> _logger;
            private readonly IConfiguration _configuration;
            private readonly IMapper _mappingService;
            private readonly UserService _userService;
            private readonly ItemService _itemService;

            public ItemController(ILogger<UserController> logger,
                                  IConfiguration configuration,
                                  IMapper mappingService,
                                  UserService userService,
                                  ItemService itemService)
            {
                this._logger = logger;
                this._configuration = configuration;
                this._userService = userService;
                this._itemService = itemService;
                this._mappingService = mappingService;
            }

            [HttpGet("GetItem")]
            public async Task<IActionResult> getItemList()
            {
                var _itemsList = await _itemService.GetAsync();
                if (_itemsList.Count() == 0)
                {
                    return BadRequest(new
                    {
                        Error = "Fail",
                        Message = "No item exist"
                    });
                }
                return Ok(new
                {
                    Message = "Successfully get all items",
                    Content = _itemsList
                });
            }

            [HttpGet("GetItem/{id}")]
            public async Task<IActionResult> getItem(string id)
            {
                var _itemsList = await _itemService.GetAsync(id);
                return Ok(new
                {
                    Message = "Successfully get this item",
                    Content = _itemsList
                });
            }

            [HttpPost("AddItem")]
            public async Task<IActionResult> addRole([FromBody] AddItemRequest newItemData)
            {
                var _itemObject = _mappingService.Map<ItemModel>(newItemData);
                await _itemService.CreateAsync(_itemObject);

                var _result = await _itemService.SearchItemviaName(_itemObject.ItemName);
                if (_result == null)
                {
                    return BadRequest(new
                    {
                        Error = "Fail",
                        Message = "Cannot create item"
                    });
                }
                return Ok(new
                {
                    Message = "Create item successfully",
                    Content = _result
                });
            }

            [HttpDelete("DeleteItem/{id}")]
            public async Task<IActionResult> deleteItem(string id)
            {
                if (await _itemService.GetAsync(id) == null) return NotFound();                
                await _itemService.DeleteAsync(id);

                return Ok(new
                {
                    Message = "Item has been deleted"
                });
            }

        [HttpPost("UploadItemImage/{id}")]
        public async Task<ActionResult> uploadProfileImage(string id, IFormFile file)
        {
            try
            {
                var _result = await _userService.uploadProfileImage(id, file);
                return Ok(new
                {
                    Message = "Uploaded profile image",
                    Content = _result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = ex.Message
                });
            }
        }
    }
}
