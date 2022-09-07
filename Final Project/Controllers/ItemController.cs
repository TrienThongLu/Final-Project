using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;

namespace Final_Project.Controllers
{
        [ApiController]
        [Route("[controller]")]
        public class ItemController : ControllerBase
        {  
            private readonly ILogger<UserController> _logger;
            private readonly IConfiguration _configuration;
            private readonly IMapper _mappingService;
            private readonly UserService _userService;
            private readonly ItemService _itemService;
            private readonly ImageService _imageService;

            public ItemController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              ItemService itemService,
                              ImageService imageService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._itemService = itemService;
            this._mappingService = mappingService;
            this._imageService = imageService;
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
        public async Task<IActionResult> addItem([FromForm] AddItemRequest newItemData)
        {
            var _itemObject = _mappingService.Map<ItemModel>(newItemData);
            await _itemService.CreateAsync(_itemObject);
            var _result = await _itemService.SearchItemviaName(_itemObject.ItemName);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create Item"
                });
            }
            await _imageService.uploadImage(_result.Id, newItemData.Image);
            return Ok(new
            {
                Message = "Create item successfully"
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

        [HttpPut("UpdateItem")]
        public async Task<IActionResult> updateItem([FromForm] UpdateItemRequests updateInfo)
        {
            var updateItem = await _itemService.GetAsync(updateInfo.ItemId);
            if (updateItem == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Item not exist"
                });
            }
            updateItem = _mappingService.Map<UpdateItemRequests, ItemModel>(updateInfo, updateItem);
            await _imageService.deleteImage(updateInfo.ItemId);
            await _itemService.UpdateAsync(updateInfo.ItemId, updateItem);
            var _result = await _itemService.SearchItemviaName(updateItem.ItemName);
            await _imageService.uploadImage(_result.Id, updateInfo.Image);

            return Ok(new
            {
                Message = "Update Item successfully"
            });
        }
    }
}
