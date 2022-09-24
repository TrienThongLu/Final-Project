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
        private readonly ToppingService _toppingService;
        public OrderController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              ItemService itemService,
                              ImageService imageService,
                              RoleService roleService,
                              ItemTypeService itemTypeService,
                              OrderService oderService,
                              ToppingService toppingService)
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
            this._toppingService = toppingService;
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
            /*_oderObject.CreatedDate = DateTime.Now;*/
            
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
        [HttpGet("getFileOrder/{id}")]
        public async Task<FileContentResult> getorder(string id)
        {
            
            var orderdata = await _oderService.GetAsync(id);
            var userdata = await _userService.GetAsync(orderdata.UserId);
            Document document = new Document();
            DocumentBuilder builder = new DocumentBuilder(document);
            builder.Writeln("ID Order : " + orderdata.Id);
            builder.Writeln("Status : " + orderdata.Status);
            builder.Writeln("UserId : " + orderdata.UserId);
            builder.Writeln("UserName : " + userdata.Fullname);
            foreach(var oder in orderdata.Items)
            {
                var dataItem = await _itemService.GetAsync(oder.id);
                builder.Writeln("Id :" + dataItem.Id);
                builder.Writeln("Name :" + dataItem.Name);
                builder.Writeln("Price :" + dataItem.Price);
                builder.Writeln("Size :" + oder.GroupSize);
                if(dataItem.Topping != null)
                foreach(var topping in oder.Topping)                 
                {
                    var toppingdata = await _toppingService.GetAsync(topping);
                    builder.Writeln("Id :" + toppingdata.Id);
                    builder.Writeln("Name :" + toppingdata.Name);
                    builder.Writeln("Price :" + toppingdata.Price);
                }
            }
            builder.Writeln("Note : " + orderdata.Note);
            builder.Writeln("TotalPrice : " + orderdata.TotalPrice);
            builder.Writeln("PurchasedDate : " + orderdata.CreatedDate);

            MemoryStream ms = new MemoryStream();
            document.Save(ms, SaveFormat.Docx);
            var result = ms.ToArray();
            return new FileContentResult(result, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName= orderdata.Id
            };
        }
    }
}
