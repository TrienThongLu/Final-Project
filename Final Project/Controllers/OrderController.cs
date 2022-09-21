using Final_Project.Models;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Final_Project.Requests.OderRequests;
using MongoDB.Driver;
using Aspose.Words;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly ItemService _itemService;
        private readonly ImageService _imageService;
        private readonly RoleService _roleService;
        private readonly ItemTypeService _itemTypeService;
        private readonly OrderService _oderService;
        public OrderController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              ItemService itemService,
                              ImageService imageService,
                              RoleService roleService,
                              ItemTypeService itemTypeService,
                              OrderService oderService)
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
        [HttpGet("GetOrder")]
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
                Message = "Successfully get all order",
                Content = _odersList
            });
        }

        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> getOder(string id)
        {
            var _odersList = await _oderService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this Oder",
                Content = _odersList
            });
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> createOder([FromBody] CreateOrderRequest newOder)
        {
            var _oderObject = _mappingService.Map<OrderModel>(newOder);
            _oderObject.CreatedDate = DateTime.Now;   
            
            await _oderService.CreateAsync(_oderObject);
            var _result = await _oderService.GetAsync(_oderObject.Id);
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
        [HttpDelete("DeleteOrder/{id}")]
        public async Task<IActionResult> deleteOrder(string id)
        {
            if (await _oderService.GetAsync(id) == null) return NotFound();
            await _itemService.DeleteAsync(id);
            await _imageService.deleteImage(id);
            return Ok(new
            {
                Message = "Item has been deleted"
            });
        }

        [HttpPut("UpdateOrder")]
        public async Task<IActionResult> updateOder([FromForm] UpdateOrderRequest updateInfo)
        {
            var updateOder = await _oderService.GetAsync(updateInfo.OderId);
            if (updateOder == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Order not exist"
                });
            }
            updateOder = _mappingService.Map<UpdateOrderRequest, OrderModel>(updateInfo, updateOder);
            await _oderService.UpdateAsync(updateInfo.OderId, updateOder);
            return Ok(new
            {
                Message = "Update Item successfully"
            });
        }

        ///////Dowload oder
        //[HttpGet("getOrder/{id}")]
        //public async Task<IActionResult> getorder(string id)
        //{
        //    var data = await _oderService.GetAsync(id);
        //    if(data == null) { return NotFound(); }
        //    Document document = new Document();
        //    DocumentBuilder builder = new DocumentBuilder(document);
        //    builder.Write(data.Status);
        //}
    } 
}
