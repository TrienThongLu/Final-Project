using Final_Project.Models;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Final_Project.Requests.OrderRequests;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Final_Project.Requests.Query;
using Scriban;
using Microsoft.AspNetCore.Authorization;
using Final_Project.Utils.Services;
using static Final_Project.Utils.Services.PayPalService;

namespace Final_Project.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private long[] pointRange = { 5000, 3000, 1000 };
        private string[] rankRange = { "Diamond", "Gold", "Silver" };

        private JObject? _momoJSON = new JObject();
        private PayPalObject? _ppObject = new PayPalObject();
        private string _payPalLink;
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
        private readonly PayPalService _payPalService;
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
                              PayPalService payPalService,
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
            this._payPalService = payPalService;
            this._storeService = storeService;
        }

        [HttpGet("GetOrder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getOrderList([FromQuery] AdminGetOrdersPR query)
        {
            return Ok(await _orderService.GetAsync(query));
        }

        [HttpGet("GetOrdersAtStore/{storeId}")]
        [Authorize(Roles = "Reception Staff, Barista, Admin")]
        public async Task<IActionResult> getOrdersAtStore(string storeId, [FromQuery] StaffGetOrdersPR query)
        {
            return Ok(await _orderService.GetAsync(storeId, query));
        }

        [HttpGet("GetOrder/{sId}")]
        public async Task<IActionResult> getOrder(string sId)
        {
            var _ordersList = await _orderService.GetOrderAsync(sId);

            return Ok(new
            {
                Message = "Successfully get this Order",
                Content = _ordersList
            });
        }

        [HttpGet("GetOrders/{storeId}")]
        public async Task<IActionResult> getOrdersByStoreId(string storeId)
        {
            var orderQuery = await _orderService.orderCollection.Find(o => o.StoreId == storeId && o.Status > 0 && o.Status < 3).ToListAsync();
            var orderUnpaid = await _orderService.GetUnpaidOrdersAsync(storeId);
            orderQuery.AddRange(orderUnpaid);
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
                                 order = g.OrderByDescending(g => g.o.IsDone).ThenBy(g => g.o.CreatedDate).Select(g => new
                                 {
                                     sId = g.o.sId,
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

            var completedOrderQuery = await _orderService.GetTop10CompletedOrdersAsync(storeId);

            var _completedOrderList = from o in completedOrderQuery
                                      join u in userQuery
                                      on o.TakenBy equals u.Id into uList
                                      from u in uList.DefaultIfEmpty()
                                      select new
                                      {
                                        sId = o.sId,
                                        Type = o.Type == 1 ? "At Store" : "Online",
                                        Status = o.Status,
                                        Amount = o.Amount,
                                        IsPaid = o.IsPaid,
                                        CreatedDate = o.CreatedDate,
                                        TakenBy = new
                                        {
                                            id = o.TakenBy != null ? o.TakenBy : string.Empty,
                                            name = o.TakenBy != null ? u.Fullname : string.Empty,
                                        },
                                        PaymentMethod = o.PaymentMethod,
                                        IsDone = o.IsDone,
                                        Address = (o.CustomerInfo != null && o.CustomerInfo.Address != null) ? o.CustomerInfo.Address : string.Empty,
                                      };

            return Ok(new
            {
                Message = "Successfully get order list",
                OrdersData = _orderList,
                CompletedOrders = _completedOrderList,
            });
        }

        [HttpGet("UserGetOrders/{id}")]
        public async Task<IActionResult> userGetOrders([FromQuery] UserGetOrdersPR query, string id)
        {

            return Ok(new
            {
                Message = "Successfully get order list",
                Data = await _orderService.UserGetOrdersAsync(query, id)
            });
        }

        [HttpGet("GetOrdersPrc/{storeId}")]
        public async Task<IActionResult> getOrdersPrc(string storeId)
        {
            var ordersList = await _orderService.orderCollection.Find(o => o.StoreId == storeId && o.Status == 1 && !o.IsDone).ToListAsync();
            var customerRole = await _roleService.RetrieveStoreRolesId();
            var userQuery = _userService.userCollection.AsQueryable().Where(u => customerRole.Contains(u.RoleId));

            var _OrderList = from o in ordersList
                                        join u in userQuery
                                        on o.TakenBy equals u.Id into uList
                                        from u in uList.DefaultIfEmpty()
                                        select new
                                        {
                                            sId = o.sId,
                                            Type = o.Type == 1 ? "At Store" : "Online",
                                            Status = o.Status,
                                            Amount = o.Amount,
                                            IsPaid = o.IsPaid,
                                            CreatedDate = o.CreatedDate,
                                            TakenBy = new
                                            {
                                                id = o.TakenBy != null ? o.TakenBy : string.Empty,
                                                name = o.TakenBy != null ? u.Fullname : string.Empty,
                                            },
                                            PaymentMethod = o.PaymentMethod,
                                            IsDone = o.IsDone,
                                            Address = (o.CustomerInfo != null && o.CustomerInfo.Address != null) ? o.CustomerInfo.Address : string.Empty,
                                        };

            return Ok(new
            {
                Message = "Successfully get order list",
                Orders = _OrderList
            });
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> createOrder([FromBody] CreateOrderRequest newOrder)
        {
            var _orderObject = _mappingService.Map<OrderModel>(newOrder);
            _orderObject.Status = 1;
            var _code = "";
            Random rd = new Random();
            for (int i = 0; i < 6; i++)
            {
                _code += rd.Next(0, 9).ToString();
            }
            _orderObject.sId = _code + DateTime.Now.ToString("yyMMddHHmmss");

            _orderObject.IsPaid = false;
            if (_orderObject.PaymentMethod != "COD" && _orderObject.PaymentMethod != "Cash")
            {
                _orderObject.Status = 0;
                if (_orderObject.PaymentMethod.Contains("MoMo"))
                {
                    _orderObject.PaymentInfo.MoMoRequestId = Guid.NewGuid().ToString();
                    switch (_orderObject.PaymentMethod)
                    {
                        case "MoMoQr":
                            _momoJSON = await _momoService.createMoMoQrRequest(_orderObject.Amount, _orderObject.sId, _orderObject.PaymentInfo.MoMoRequestId);
                            break;
                        case "MoMoATM":
                            _momoJSON = await _momoService.createMoMoATMRequest(_orderObject.Amount, _orderObject.sId, _orderObject.PaymentInfo.MoMoRequestId);
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

                if (_orderObject.PaymentMethod == "PayPal")
                {
                    _ppObject = await _payPalService.createPayPalRequest(_orderObject);

                    if (_ppObject == null || string.IsNullOrEmpty(_ppObject.Url) || string.IsNullOrEmpty(_ppObject.Id))
                    {
                        return BadRequest(new
                        {
                            Error = "Fail",
                            Message = "Cannot create pp"
                        });
                    }

                    _orderObject.PaymentInfo.PPPayId = _ppObject.Id;
                }
            }

            _orderObject.CreatedDate = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
            await _orderService.CreateAsync(_orderObject);
            var _result = await _orderService.GetAsync(_orderObject.sId);
            if (_result == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Cannot create Order"
                });
            }
            if (_momoJSON != null && _orderObject.PaymentMethod.Contains("MoMo") && !string.IsNullOrEmpty(_momoJSON.ToString()))
            {
                JToken token = JObject.Parse(_momoJSON.ToString());
                return Ok(new
                {
                    Message = "Create order Momo successfully",
                    PayUrl = (string)token.SelectToken("payUrl")
                });
            }
            if (_ppObject != null && _orderObject.PaymentMethod == "PayPal" && !string.IsNullOrEmpty(_ppObject.Url) && !string.IsNullOrEmpty(_ppObject.Id))
            {
                return Ok(new
                {
                    Message = "Create order paypal successfully",
                    PayUrl = _ppObject.Url,
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
            if (_result.Status == -1 && _result.PaymentInfo.MoMoRequestId != response.requestId && 
                (response.partnerCode != _configuration.GetSection("MoMoPaymentQr").GetValue<string>("partnerCode") || response.partnerCode != _configuration.GetSection("MoMoPaymentATM").GetValue<string>("partnerCode")))
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Payment invalid"
                });
            }

            _result.PaymentInfo.MoMoTransId = response.transId;

            if (response.resultCode != 0)
            {
                _result.Status = -1;

                await _orderService.UpdateAsync(_result.sId, _result);

                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Order failed"
                });
            }

            _result.IsPaid = true;
            _result.Status = 1;

            await _orderService.UpdateAsync(_result.sId, _result);

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpPut("PayPalTransaction")]
        public async Task<IActionResult> payPalTransaction([FromBody] PayPalPaymentResponse response)
        {
            var _result = await _orderService.GetAsync(response.orderId);
            if (_result.Status == -1)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Payment invalid"
                });
            }


            _result.PaymentInfo.PPToken = response.PPToken;

            if (string.IsNullOrEmpty(response.PPPayer) || string.IsNullOrEmpty(response.PPPayId))
            {
                _result.Status = -1;

                await _orderService.UpdateAsync(_result.sId, _result);

                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Order failed"
                });
            }

            if (!string.IsNullOrEmpty(response.PPPayer) && !string.IsNullOrEmpty(response.PPPayId) && response.PPPayId == _result.PaymentInfo.PPPayId)
            {
                _result.IsPaid = true;
                _result.Status = 1;

                _result.PaymentInfo.PPPayer = response.PPPayer;

                await _payPalService.ApprovePayPal(response.PPPayId, response.PPPayer);

                await _orderService.UpdateAsync(_result.sId, _result);
            }

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpPut("OrderDone/{sId}")]
        public async Task<IActionResult> orderDone(string sId)
        {
            var _result = await _orderService.GetAsync(sId);

            if (_result == null || _result.Status == -1 || _result.IsDone)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Finish order failed"
                });
            }

            _result.IsDone = true;
            await _orderService.UpdateAsync(sId, _result);

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpPut("NextStatus/{sId}")]
        public async Task<IActionResult> nextStatus(string sId)
        {
            var _result = await _orderService.GetAsync(sId);
            if (_result == null || _result.Status == -1 || !_result.IsDone)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Update status failed"
                });
            }

            if (_result.Status == 1)
            {
                if (_result.Type == 1)
                {
                    if (!_result.IsPaid)
                    {
                        _result.Status = 0;
                    }
                    else
                    {
                        _result.Status = 3;
                    }
                    await _orderService.UpdateAsync(sId, _result);
                } else
                {
                    _result.Status = 2;
                    await _orderService.UpdateAsync(sId, _result);
                }
                return Ok(new
                {
                    Message = "Update order successfully",
                });
            }

            return BadRequest(new
            {
                Error = "Fail",
                Message = "Update status failed"
            });
        }

        [HttpPut("CompleteOrder/{sId}")]
        public async Task<IActionResult> completeOrder(string sId)
        {
            var _result = await _orderService.GetAsync(sId);
            if (_result == null || _result.Status == -1 || !_result.IsDone)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Update status failed"
                });
            }

            _result.Status = 3;
            _result.IsPaid = true;

            await _orderService.UpdateAsync(sId, _result);

            if (_result.CustomerInfo != null && _result.CustomerInfo.Id != null)
            {
                var _userData = await _userService.GetAsync(_result.CustomerInfo.Id);
                double amount = (_result.Amount) / 1000;
                _userData.Point += (long)Math.Round(amount);

                foreach (var point in pointRange)
                {
                    if (_userData.Point > point)
                    {
                        int index = Array.IndexOf(pointRange, point);
                        _userData.Ranking = rankRange[index];
                        break;
                    }
                }

                await _userService.UpdateAsync(_userData.Id, _userData);
            }

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpPut("OrderFailed/{sId}")]
        public async Task<IActionResult> orderFailed(string sId)
        {
            var _result = await _orderService.GetAsync(sId);
            if (_result == null || _result.Status == -1)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Update status failed"
                });
            }

            _result.Status = -1;

            await _orderService.UpdateAsync(sId, _result);

            return Ok(new
            {
                Message = "Update order successfully",
            });
        }

        [HttpDelete("DeleteOrder/{sId}")]
        public async Task<IActionResult> deleteOrder(string sId)
        {
            if (await _orderService.GetAsync(sId) == null) return NotFound();
            await _orderService.DeleteAsync(sId);
            return Ok(new
            {
                Message = "Order has been deleted"
            });
        }

        [HttpPut("ChangeCustomer")]
        public async Task<IActionResult> changeCustomer([FromBody] ChangeCusRequest request)
        {
            var _result = await _orderService.GetAsync(request.OrderId);
            if (_result == null || _result.Status == -1)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "Update status failed"
                });
            }

            var _userData = await _userService.GetAsync(request.UserId);
            if (_userData == null)
            {
                return BadRequest(new
                {
                    Error = "Fail",
                    Message = "User not exist"
                });
            }

            _result.TakenBy = request.UserId;

            await _orderService.UpdateAsync(_result.sId, _result);

            return Ok(new
            {
                Message = "Order updated"
            });
        }

        [HttpGet("getFileOrder/{sId}")]
        public async Task<FileContentResult> getFileOrder(string sId)
        {
            var orderData = await _orderService.GetAsync(sId);
            var StoreData = await _storeService.GetAsync(orderData.StoreId);
            var ItemData = await _itemService.GetAsync();
            var templateContent = System.IO.File.ReadAllText("./Invoice/htmlpage.html");
            var template = Template.Parse(templateContent);
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
                }
               
                items.Add(item); 
            }
          
             dynamic GenerateDataDemoAsync()
            {
                var user = new
                {
                    Phone = (orderData.CustomerInfo != null && orderData.CustomerInfo.Phonenumber != null)? orderData.CustomerInfo.Phonenumber : String.Empty,
                };
                var shippfee = new
                {
                    Fee = (orderData.PaymentInfo != null && orderData.PaymentInfo.ShippingFee != null) ? orderData.PaymentInfo.ShippingFee : 0,
                };
                var order = new
                {
                    Id = orderData.sId,
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
                        Shippingfee= shippfee,
                        Total = orderData.TotalPrice,
                        Amount = orderData.Amount,
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
            PDF.SaveAs("./pdfbill/" + Datetime+ "/" +orderData.sId+ ".pdf");
            var result = System.IO.File.ReadAllBytes("./pdfbill/" + Datetime + "/" + orderData.sId + ".pdf");
            return new FileContentResult(result, "application/pdf")
            {
                FileDownloadName = orderData.sId + ".pdf"
            };
        }

        [HttpGet("Statistic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> statistic([FromQuery] StatisticQuery query)
        {
            var from = new DateTime();
            var to = new DateTime();
            TimeSpan tsFrom = new TimeSpan(0,0,0);
            TimeSpan tsTo = new TimeSpan(23,59,59);
            if (string.IsNullOrEmpty(query.from) || string.IsNullOrEmpty(query.to))
            {
                to = DateTime.Now;
                from = to.AddMonths(-1);
            } else
            {
                from = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(query.from));
                to = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(query.to));
            }

            long fromMili = new DateTimeOffset(from.Date + tsFrom).ToUnixTimeMilliseconds();
            long toMili = new DateTimeOffset(to.Date + tsTo).ToUnixTimeMilliseconds();

            var storeQuery = _storeService.StoreCollection.AsQueryable();
            var orderQueryThisMonth = _orderService.orderCollection.Find(o => o.CreatedDate >= fromMili && o.CreatedDate <= toMili).ToList();

            var _revenueEachStore = from o in orderQueryThisMonth
                                    join s in storeQuery
                                    on o.StoreId equals s.Id
                                    group new { o, s } by o.StoreId into g
                                    select new
                                    {
                                        storeName = g.First().s.Name,
                                        revenue = g.Sum(g => g.o.Amount),
                                        totalOrder = g.Count()
                                    };


            var _itemTopQuery = _orderService.orderCollection.AsQueryable().Where(o => o.CreatedDate >= fromMili && o.CreatedDate <= toMili).SelectMany(o => o.Items);

            var _Top5PurchasedItem = from o in _itemTopQuery
                         group o by o.Name into gr
                         select new
                         {
                             name = gr.Key,
                             count = gr.Sum(i => i.Quantity)
                         };

            var total = new
            {
                tUser = await _userService.GetTotalAsync(),
                tOrder = await _orderService.GetTotalAsync(),
                tRev = await _orderService.GetTotalRevAsync(),
            };

            var orderSt = await _orderService.GetTotalOrderStAsync(fromMili, toMili);

            var onCusId = await _roleService.RetrieveOnlineCustomerRole();
            var topUserQuery = _userService.userCollection.AsQueryable().Where(u => u.RoleId == onCusId.Id && u.Point > 0).OrderByDescending(u => u.Point).Select(u => new
            {
                u.PhoneNumber,
                u.Fullname,
                u.Point,
                u.Ranking
            }).Take(5);

            return Ok(new
            {
                top5PurItems = _Top5PurchasedItem.OrderByDescending(g => g.count).Take(5),
                revEachStore = _revenueEachStore.OrderByDescending(o => o.revenue),
                total = total,
                orderSt = orderSt,
                topUser = topUserQuery,

                from = fromMili,
                to = toMili
            });
        }

        [HttpGet("OnlinePayDetail/{sId}")]
        [AllowAnonymous]
        public async Task<IActionResult> getPayDetail(string sId)
        {
            var orderObject = await _orderService.GetAsync(sId);
            if (orderObject == null) return NotFound();

            if (orderObject.PaymentMethod.Contains("MoMo"))
            {
                var detail = await _momoService.queryRequest(orderObject.PaymentMethod, orderObject.PaymentInfo.MoMoRequestId, orderObject.sId);
                JToken token = JObject.Parse(detail.ToString());

                return Ok(new
                {
                    type = "MoMo",
                    orderId = (string)token.SelectToken("orderId"),
                    requestId = (string)token.SelectToken("requestId"),
                    amount = (string)token.SelectToken("amount"),
                    transId = (string)token.SelectToken("transId"),
                    payType = (string)token.SelectToken("payType"),
                    message = (string)token.SelectToken("message"),
                });
            }

            if (orderObject.PaymentMethod == "PayPal")
            {
                var _payPalObject = await _payPalService.GetPayPalDetail(orderObject.PaymentInfo.PPPayId);

                return Ok(new
                {
                    type = "PayPal",
                    Id = _payPalObject.Id,
                    CreatedTime = _payPalObject.CreatedTime,
                    State = _payPalObject.State,
                    Amount = _payPalObject.Amount,
                    Payer = _payPalObject.Payer
                });
            }

            return BadRequest(new
            {
                Error = "Fail",
            });
        }

        /*[HttpGet("getStatisticStoreThisMonth")]
        public async Task<IActionResult> getRevenueOfEachStore()
        {
            var date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            long firstdaymili = new DateTimeOffset(firstDayOfMonth).ToUnixTimeMilliseconds();
            long lastdaymili = new DateTimeOffset(lastDayOfMonth).ToUnixTimeMilliseconds();

            var storeQuery = _storeService.StoreCollection.AsQueryable();
            var orderQuery = _orderService.orderCollection.Find(o => o.CreatedDate >= firstdaymili && o.CreatedDate <= lastdaymili).ToList();

            var _result = from o in orderQuery
                          join s in storeQuery
                          on o.StoreId equals s.Id
                          group new { o, s } by o.StoreId into g
                          select new
                          {
                              storeName = g.First().s.Name,
                              revenue = g.Sum(g => g.o.Amount),
                              totalOrder = g.Count()
                          };

            return Ok(new
            {
                Message = "Successfully get revenue of each store ",
                revenue = _result.OrderByDescending(o => o.revenue),
            });
        }*/

        /*[HttpGet("getRevenueThisMonth")]
        public async Task<IActionResult> getRevenueMonthOfStore()
        {
            var storedata = await _storeService.GetAsync();
            var date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            long firstdaymili = new DateTimeOffset(firstDayOfMonth).ToUnixTimeMilliseconds();
            long lastdaymili = new DateTimeOffset(lastDayOfMonth).ToUnixTimeMilliseconds();
            List<dynamic> Revenue = new List<dynamic>();
            foreach ( var _store in storedata)
            {
                long revenue = 0;
                var _completedOrderList = await _orderService.GetCompletedOrdersAsync(_store.Id);
                var data = _completedOrderList.Where(x => x.CreatedDate <= lastdaymili && x.CreatedDate >= firstdaymili);
                foreach (var _order in data)
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
                Message = "Successfully get revenue this month of store ",
                Revenue = Revenue.ToList()
            });
        }*/

        /*[HttpGet("getTotalOrderIsDoneThisMonth")]
        public async Task<IActionResult> getTotalOrderIsDone()
        {
            var storedata = await _storeService.GetAsync();
            var date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            long firstdaymili = new DateTimeOffset(firstDayOfMonth).ToUnixTimeMilliseconds();
            long lastdaymili = new DateTimeOffset(lastDayOfMonth).ToUnixTimeMilliseconds();
            List<dynamic> OrderIsDone = new List<dynamic>();
            foreach (var _store in storedata)
            {
                long revenue = 0;
                var _completedOrderList = await _orderService.GetCompletedOrdersAsync(_store.Id);
                var data = _completedOrderList.Where(x => x.CreatedDate <= lastdaymili && x.CreatedDate >= firstdaymili).Count();             
                var store = new
                {
                    Name = _store.Name,
                    OrderIsDone = data
                };
                OrderIsDone.Add(store);
            }

            return Ok(new
            {
                Message = "Successfully get revenue this month of store ",
                OrderIsDone = OrderIsDone.ToList()
            });
        }*/

        /*[HttpGet("getRevenueThisMonth/{id}")]
         public async Task<IActionResult> getRevenueMonthOfStore(string id)
         {
             var storedata = await _storeService.GetAsync(id);          
             var date = DateTime.Now;
             var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
             var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
             long firstdaymili = new DateTimeOffset(firstDayOfMonth).ToUnixTimeMilliseconds();
             long lastdaymili = new DateTimeOffset(lastDayOfMonth).ToUnixTimeMilliseconds();
             long revenue = 0;
             var _completedOrderList = await _orderService.GetCompletedOrdersAsync(id);
             var data = _completedOrderList.Where(x => x.CreatedDate <= lastdaymili && x.CreatedDate >= firstdaymili);
                 foreach (var _order in data)
                 {
                     revenue += _order.Amount;
                 }
             var store = new
             {
                 Name = storedata.Name,
                 Revenue = revenue
             };

             return Ok(new
             {
                 Message = "Successfully get revenue this month of store ",     
                 Revenue = revenue
             });
         }*/


        /*[HttpGet("getTotalOrderIsDoneThisMonth/{id}")]
        public async Task<IActionResult> getTotalOrderIsDone(string id)
        {
            var storedata = await _storeService.GetAsync(id);
            var date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            long firstdaymili = new DateTimeOffset(firstDayOfMonth).ToUnixTimeMilliseconds();
            long lastdaymili = new DateTimeOffset(lastDayOfMonth).ToUnixTimeMilliseconds();
            long revenue = 0;
            var _completedOrderList = await _orderService.GetCompletedOrdersAsync(id);
            var data = _completedOrderList.Where(x => x.CreatedDate <= lastdaymili && x.CreatedDate >= firstdaymili).Count();

            return Ok(new
            {
                Message = "Successfully get revenue this month of store ",
                Total = "Total Order is Done in this Month :" + data,
            });
        }*/


    }
}
