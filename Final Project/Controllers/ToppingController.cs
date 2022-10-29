using Final_Project.Models;
using Final_Project.Services;
using Final_Project.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Final_Project.Requests.RoleRequests;
using AutoMapper;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.ToppingRequests;
using Microsoft.AspNetCore.Authorization;

namespace Final_Project.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly ItemService _itemService;
        private readonly ImageService _imageService;
        public readonly ToppingService _toppingService;

        public ToppingController(ILogger<UserController> logger,
                          IConfiguration configuration,
                          IMapper mappingService,
                          UserService userService,
                          ItemService itemService,
                          ImageService imageService,
                          ToppingService toppingService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._itemService = itemService;
            this._mappingService = mappingService;
            this._imageService = imageService;
            this._toppingService = toppingService;
        }

        [HttpGet("GetTopping")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getToppingList()
        {
            var _toppingsList = await _toppingService.GetAsync();
            if (_toppingsList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No topping exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all topping",
                Content = _toppingsList
            });
        }

        [HttpGet("GetTopping/{id}")]
        public async Task<IActionResult> getTopping(string id)
        {
            var _toppingsList = await _toppingService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this topping",
                Content = _toppingsList
            });
        }

        [HttpPost("AddTopping")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> addTopping([FromBody] AddToppingRequest newTopping)
        {
            var _toppingObject = _mappingService.Map<ToppingModel>(newTopping);
            await _toppingService.CreateAsync(_toppingObject);
            var _result = await _toppingService.SearchToppingviaName(_toppingObject.Name);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create Topping"
                });
            }       
            return Ok(new
            {
                Message = "Create topping successfully"
            });
        }

        [HttpDelete("DeleteTopping/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> deleteToppping(string id)
        {
            try
            {
                if (await _toppingService.GetAsync(id) == null) return NotFound();    
                await _toppingService.DeleteAsync(id);
                return Ok(new
                {
                    Message = "This topping has been deleted"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot delete topping"
                });
            }
        }

        [HttpPut("UpdateTopping")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> updateTopping([FromForm] UpdateToppingRequest updateInfo)
        {
            var updateTopping = await _toppingService.GetAsync(updateInfo.Id);
            if (updateTopping == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Topping not exist"
                });
            }
            updateTopping = _mappingService.Map<UpdateToppingRequest, ToppingModel>(updateInfo, updateTopping);           
            var _result = await _toppingService.SearchToppingviaName(updateTopping.Name);         
            return Ok(new
            {
                Message = "Update Topping successfully"
            });
        }
    }
}
