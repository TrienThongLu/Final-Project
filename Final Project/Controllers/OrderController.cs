﻿using Final_Project.Models;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Final_Project.Requests.OrderRequests;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Final_Project.Requests.Query;
using Scriban;

namespace Final_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private JObject? _momoJSON = new JObject();
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mappingService;
        private readonly UserService _userService;
        private readonly ItemService _itemService;
        private readonly ImageService _imageService;
        private readonly RoleService _roleService;
        private readonly ItemTypeService _itemTypeService;
        private readonly OrderService _orderService;
        private readonly ToppingService _toppingService;
        private readonly MomoService _momoService;
        private readonly StoreLocationService _storeService;

        public OrderController(ILogger<UserController> logger,
                              IConfiguration configuration,
                              IMapper mappingService,
                              UserService userService,
                              ItemService itemService,
                              ImageService imageService,
                              RoleService roleService,
                              ItemTypeService itemTypeService,
                              OrderService orderService,
                              ToppingService toppingService,
                              MomoService momoService,
                              StoreLocationService storeService)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._userService = userService;
            this._itemService = itemService;
            this._mappingService = mappingService;
            this._imageService = imageService;
            this._roleService = roleService;
            this._itemTypeService = itemTypeService;
            this._orderService = orderService;
            this._toppingService = toppingService;
            this._momoService = momoService;
            this._storeService = storeService;
        }
        [HttpGet("GetOrder")]
        public async Task<IActionResult> getOrderList()
        {
            var _ordersList = await _orderService.GetAsync();
            if (_ordersList.Count() == 0)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "No order exist"
                });
            }
            return Ok(new
            {
                Message = "Successfully get all order",
                Content = _ordersList
            });
        }

        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> getOrder(string id)
        {
            var _ordersList = await _orderService.GetAsync(id);
            return Ok(new
            {
                Message = "Successfully get this Order",
                Content = _ordersList
            });
        }

        [HttpGet("GetOrders/{storeId}")]
        public async Task<IActionResult> getOrdersByStoreId(string storeId)
        {
            var orderQuery = await _orderService.orderCollection.Find(o => o.StoreId == storeId && o.Status > 0 && o.Status < 4).SortBy(o => o.Status).ToListAsync();
            var customerRole = await _roleService.RetrieveStoreRolesId();
            var userQuery = _userService.userCollection.AsQueryable().Where(u => customerRole.Contains(u.RoleId));

            var _orderList = from o in orderQuery
                             join u in userQuery
                             on o.TakenBy equals u.Id into uList
                             from u in uList.DefaultIfEmpty()
                             group new {o, u} by o.Status into g
                             select new
                             {
                                 status = g.Key,
                                 order = g.OrderBy(g => g.o.CreatedDate).ThenBy(g => g.o.IsDone).Select(g => new
                                 {
                                     Id = g.o.Id,
                                     Type = g.o.Type == 1 ? "At Store" : "Online",
                                     Status = g.o.Status,
                                     Amount = g.o.Amount,
                                     IsPaid = g.o.IsPaid,
                                     CreatedDate = g.o.CreatedDate,
                                     TakenBy = new
                                     {
                                         id = g.o.TakenBy != null ? g.o.TakenBy : string.Empty,
                                         name = g.o.TakenBy != null ? g.u.Fullname : string.Empty,
                                     },
                                     PaymentMethod = g.o.PaymentMethod,
                                     IsDone = g.o.IsDone,
                                     Address = (g.o.CustomerInfo != null && g.o.CustomerInfo.Address != null) ? g.o.CustomerInfo.Address : string.Empty,
                                 })
                             };

            var _completedOrderList = await _orderService.GetTop5CompletedOrdersAsync(storeId);

            return Ok(new
            {
                Message = "Successfully get order list",
                OrdersData = _orderList,
                CompletedOrders = _completedOrderList
            });
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> createOrder([FromBody] CreateOrderRequest newOrder)
        {
            var _orderObject = _mappingService.Map<OrderModel>(newOrder);
            _orderObject.Id = ObjectId.GenerateNewId().ToString();
            _orderObject.Status = 1;
            if (_orderObject.Type == 1 && _orderObject.PaymentMethod == "Cash")
            {
                _orderObject.IsPaid = true;
            }
            else
            {
                _orderObject.IsPaid = false;
                if (_orderObject.PaymentMethod != "COD")
                {
                    _orderObject.Status = 0;
                    _orderObject.PaymentInfo.RequestId = Guid.NewGuid().ToString();
                    switch (_orderObject.PaymentMethod)
                    {
                        case "MoMoQr":
                            _momoJSON = await _momoService.createMoMoQrRequest(_orderObject.Amount, _orderObject.Id, _orderObject.PaymentInfo.RequestId);
                            break;
                        case "MoMoATM":
                            _momoJSON = await _momoService.createMoMoATMRequest(_orderObject.Amount, _orderObject.Id, _orderObject.PaymentInfo.RequestId);
                            break;
                    }

                    if (_momoJSON == null || string.IsNullOrEmpty(_momoJSON.ToString()))
                    {
                        return BadRequest(new
                        {
                            Error = "Fail",
                            Message = "Cannot create Order"
                        });
                    }
                }
            }
            _orderObject.CreatedDate = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
            await _orderService.CreateAsync(_orderObject);
            var _result = await _orderService.GetAsync(_orderObject.Id);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create Order"
                });
            }
            if (_momoJSON != null && !string.IsNullOrEmpty(_momoJSON.ToString()))
            {
                JToken token = JObject.Parse(_momoJSON.ToString());
                return Ok(new
                {
                    Message = "Create order successfully",
                    PayUrl = (string)token.SelectToken("payUrl")
                });
            }
            return Ok(new
            {
                Message = "Create order successfully"
            });
        }

        [HttpPut("MoMoTransaction")]
        public async Task<IActionResult> momoTransaction([FromBody] MoMoPaymentResponse response)
        {
            var _result = await _orderService.GetAsync(response.orderId);
            if (_result.Status == -1 && _result.PaymentInfo.RequestId != response.requestId && 
                (response.partnerCode != _configuration.GetSection("MoMoPaymentQr").GetValue<string>("partnerCode") || response.partnerCode != _configuration.GetSection("MoMoPaymentATM").GetValue<string>("partnerCode")))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Payment invalid"
                });
            }

            _result.PaymentInfo.TransId = response.transId;

            if (response.resultCode != 0)
            {
                _result.Status = -1;

                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Order failed"
                });
            }

            _result.IsPaid = true;
            _result.Status = 1;

            await _orderService.UpdateAsync(response.orderId, _result);

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpDelete("DeleteOrder/{id}")]
        public async Task<IActionResult> deleteOrder(string id)
        {
            if (await _orderService.GetAsync(id) == null) return NotFound();
            await _itemService.DeleteAsync(id);
            await _imageService.deleteImage(id);
            return Ok(new
            {
                Message = "Item has been deleted"
            });
        }

        [HttpGet("getFileOrder/{id}")]
        public async Task<FileContentResult> getorder(string id)
        {
            var orderData = await _orderService.GetAsync(id);
            var StoreData = await _storeService.GetAsync(orderData.StoreId);
            var ItemData = await _itemService.GetAsync();
            var UserData = await _userService.GetAsync(orderData.CustomerInfo.Id);
            var ToppingData = await _toppingService.GetAsync();
            var templateContent = System.IO.File.ReadAllText("./Invoice/htmlpage.html");
            var template = Template.Parse(templateContent);
            long totalsum = 0;
            List<dynamic> items = new List<dynamic>();
            foreach( var _item in orderData.Items)
            {
                var item = new
                {
                    Name = _item.Name,
                    Size = _item.Size,
                    Price = _item.Price,
                    Quantity = _item.Quantity,
                    Total = _item.Quantity * _item.Price,                
                    Topping = new List<dynamic>(),        
                };      
                totalsum = totalsum + item.Total;
                foreach (var _topping in _item.Topping)
                {
                    var topping = new
                    {
                        Name = _topping.Name,
                        Quantity = _topping.Quantity,                        
                        Price = _topping.Price,
                        Total = (_item.Quantity * _topping.Quantity) * _topping.Price,
                    };
                    item.Topping.Add(topping);
                    /*totalsum += item.Total + topping.Total;*/
                    totalsum += totalsum + topping.Total;
                }
               
                items.Add(item); 
            }
          
             dynamic GenerateDataDemoAsync()
            {               
                var user = new
                {
                    Phone = orderData.CustomerInfo.Phonenumber,
                };
                
                var order = new
                {
                    Id = orderData.Id,
                    //Total = orderData.TotalPrice,
                    Discount = orderData.DiscountPercent,
                    //Amount = orderData.Amount,
                };
                var data = new
                {
                    Data = new
                    {   
                        Address = StoreData.Address,
                        User = user,
                        Item = items,
                        Order = order, 
                        Total = totalsum,
                        Amount = totalsum - (totalsum * orderData.DiscountPercent / 100 ),
                        Date = DateTime.Now.ToString("M-d-yyyy"),
                    },
                };
                return data;
            }          
            var datatest = GenerateDataDemoAsync();
            var pageContent = template.Render(datatest);
            var Renderer = new IronPdf.ChromePdfRenderer();
            var PDF = Renderer.RenderHtmlAsPdf(pageContent);
            var Datetime = DateTime.Now.ToString("M-d-yyyy");
            string dir = "./pdfbill/" + Datetime;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            PDF.SaveAs("./pdfbill/" + Datetime+ "/" +orderData.Id+ ".pdf");            
            var result = System.IO.File.ReadAllBytes("./pdfbill/" + Datetime + "/" + orderData.Id + ".pdf");
            return new FileContentResult(result, "application/pdf")
            {
                FileDownloadName = orderData.Id + ".pdf"
            };          
        }

        [HttpGet("getRevenueStore")]
        public async Task<IActionResult> getRevenueOfEachStore()
        {
            var storeData = await _storeService.GetAsync();
            
            List<dynamic> Revenue = new List<dynamic>();
            foreach( var _store in storeData)
            {
                long revenue = 0;
                var _completedOrderList = await _orderService.GetCompletedOrdersAsync(_store.Id);              
                foreach (var _order in _completedOrderList)
                {                 
                    revenue += _order.Amount;
                    
                }
                var store = new
                {
                    Name = _store.Name,
                    Revenue = revenue
                };
                Revenue.Add(store);
            }                      
            return Ok(new
            {
                Message = "Successfully get revenue of each store ",   
                revenue = Revenue.OrderByDescending(x=>x.Revenue).ToList()
            });
        }

/*        [HttpGet("getRevenueMonthOfStore/{id}")]
        public async Task<IActionResult> getRevenueMonthOfStore(string id)
        {
            var storeData = await _storeService.GetAsync();

            List<dynamic> Revenue = new List<dynamic>();
            foreach (var _store in storeData)
            {
                long revenue = 0;
                var _completedOrderList = await _orderService.GetCompletedOrdersAsync(_store.Id);
                foreach (var _order in _completedOrderList)
                {
                    revenue += _order.Amount;

                }
                var store = new
                {
                    Name = _store.Name,
                    Revenue = revenue
                };
                Revenue.Add(store);
            }
            return Ok(new
            {
                Message = "Successfully get revenue of each store ",
                revenue = Revenue.OrderByDescending(x => x.Revenue).ToList()
            });
        }*/
    }
}
