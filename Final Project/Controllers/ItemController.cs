using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.Query;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

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
        private readonly OrderService _orderService;
        private readonly ToppingService _toppingService;
        private readonly ImageService _imageService;

        public ItemController(ILogger<UserController> logger,
                            IConfiguration configuration,
                            IMapper mappingService,
                            UserService userService,
                            ItemService itemService,
                            ToppingService toppingService,
                            ImageService imageService,
                            OrderService orderService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._itemService = itemService;
            this._toppingService = toppingService;
            this._mappingService = mappingService;
            this._imageService = imageService;
            this._orderService = orderService;
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
        public async Task<IActionResult> getListItems([FromQuery] ItemPR paginationRequest)
        {
            return Ok(await _itemService.GetAsync(paginationRequest));
        }

        [HttpGet("GetItembyType/{id}")]
        public async Task<IActionResult> getItembyType(string id)
        {
            try
            {
                var _itemList = _itemService.itemCollection.AsQueryable().Where(i => i.TypeId == id).Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    image = i.Image,
                    price = i.Price,
                    typeId = i.TypeId,
                    groupSizes = i.GroupSizes,
                    toppings = new List<object>(),
                    i.ToppingIds,
                });

                var _toppingList = _toppingService.toppingCollection.AsQueryable();
                var _itemData = new List<object>();
                foreach (var _item in _itemList)
                {
                    var query = from toppingId in _item.ToppingIds
                                join topping in _toppingList
                                on toppingId equals topping.Id
                                select new
                                {
                                    id = topping.Id,
                                    name = topping.Name,
                                    price = topping.Price,
                                    quantity = 0,
                                };
                    foreach (var topping in query)
                    {
                        _item.toppings.Add(topping);
                    }
                    _itemData.Add(_item);
                }

                return Ok(new
                {
                    Message = "Successfully get this item",
                    Content = _itemData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetItem/{id}")]
        public async Task<IActionResult> getItem(string id)
        {
            var _item = await _itemService.GetAsync(id);

            if (_item == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Item doesnot exist"
                });
            }

            var _toppingQuery = _toppingService.toppingCollection.AsQueryable();

            var query = from toppingId in _item.ToppingIds
                        join topping in _toppingQuery
                        on toppingId equals topping.Id
                        select new
                        {
                            id = topping.Id,
                            name = topping.Name,
                            price = topping.Price,
                            quantity = 0,
                        };

            /*var toppings = new List<Object>();

            if (_item.ToppingIds is not null && _item.ToppingIds.Any())
            {
                foreach (string topping in _item.ToppingIds)
                {
                    var toppingItem = await _toppingService.GetAsync(topping);
                    var toppingObject = new
                    {
                        id = toppingItem.Id,
                        name = toppingItem.Name,
                        price = toppingItem.Price,
                        quantity = 0
                    };
                    toppings.Add(toppingObject);
                }
            }*/

            var itemObject = new
            {
                id = _item.Id,
                name = _item.Name,
                toppings = query,
                groupSizes = _item.GroupSizes,
            };

            return Ok(new
            {
                Message = "Get item successfully",
                Content = itemObject
            });
        }

        [HttpPost("AddItem")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> addItem([FromForm] AddItemRequest newItemData)
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
    }
}
