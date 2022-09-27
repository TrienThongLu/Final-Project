﻿using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.ItemRequests;
using Final_Project.Requests.Query;

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

        /*[HttpGet("GetItem")]
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
        }*/

        [HttpGet("GetItem")]
        public async Task<IActionResult> getListItems([FromQuery] ItemPaginationRequest paginationRequest)
        {
            /*var _usersList = await _userService.GetAsync();*/
            /*return Ok(new
            {
                Message = $"Successfully get users",
                Content = _usersList
            });*/
            return Ok(await _itemService.GetAsync(paginationRequest));
        }

        [HttpPost("AddItem")]
        public async Task<IActionResult> addItem([FromBody] AddItemRequest newItemData)
        {

            var _itemObject = _mappingService.Map<ItemModel>(newItemData);

            await _itemService.CreateAsync(_itemObject);
            var _result = await _itemService.SearchItemviaName(_itemObject.Name);
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


        /*[HttpPut("UploadItemImage")]
        public async Task<IActionResult> uploadItemImage([FromForm] AddItemImageRequest uploadInfo)
        {
            var updateItem = await _itemService.GetAsync(uploadInfo.Id);
            if (updateItem == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Item not exist"
                });
            }
            updateItem = _mappingService.Map<AddItemImageRequest, ItemModel>(uploadInfo, updateItem);
            if (!(uploadInfo.Image is string) && !(uploadInfo.Image is null))
            {
                await _imageService.deleteImage(uploadInfo.Id);
            }
            await _itemService.UpdateAsync(uploadInfo.Id, updateItem);
            if (!(uploadInfo.Image is string) && !(uploadInfo.Image is null))
            {
                await _imageService.uploadImage(uploadInfo.Id, uploadInfo.Image);
            }

            return Ok(new
            {
                Message = "Upload Item image successfully"
            });
        }*/


        [HttpDelete("DeleteItem/{id}")]
        public async Task<IActionResult> deleteItem(string id)
        {
            try
            {
                if (await _itemService.GetAsync(id) == null) return NotFound();
                if (await _imageService.deleteImage(id))
                {
                    await _itemService.DeleteAsync(id);
                }
                return Ok(new
                {
                    Message = "Item has been deleted"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot delete Item"
                });
            }
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
           if (!(updateInfo.ImageUpload is string) && !(updateInfo.ImageUpload is null))
            {
                await _imageService.deleteImage(updateInfo.ItemId);
            }
            await _itemService.UpdateAsync(updateInfo.ItemId, updateItem);
           if (!(updateInfo.ImageUpload is string) && !(updateInfo.ImageUpload is null))
            {
                await _imageService.uploadImage(updateInfo.ItemId, updateInfo.ImageUpload);
            }

            return Ok(new
            {
                Message = "Update Item successfully"
            });
        }

        [HttpGet("GetItembyType/{id}")]
        public async Task<IActionResult> getItembyType(string id)
        {
            var _itemsList = await _itemService.GetlistItembytype(id);
            return Ok(new
            {
                Message = "Successfully get this item",
                Content = _itemsList
            });                
        }
    }
}
