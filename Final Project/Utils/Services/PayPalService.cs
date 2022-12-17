

using Final_Project.Models;
using Final_Project.Requests.OrderRequests;
using PayPal.Core;
using PayPal.v1.Payments;

namespace Final_Project.Utils.Services
{
    public class PayPalService
    {
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _secretKey;
        private readonly double _VNtoUSD = 24800;

        public PayPalService(IConfiguration configuration)
        {
            _configuration = configuration;
            this._clientId = configuration["PayPalSettings:ClientId"];
            this._secretKey = configuration["PayPalSettings:SecretKey"];
        }

        public class PayPalObject
        {
            public string? Url { get; set; }
            public string? Id { get; set; }
            public string? Payer { get; set; }
            public string? CreatedTime { get; set; }
            public string? State { get; set; }
            public string? Amount { get; set; }
        }

        public async Task<PayPalObject> createPayPalRequest(OrderModel order)
        {
            var environment = new SandboxEnvironment(_clientId, _secretKey);
            var client = new PayPalHttpClient(environment);

            var itemList = new ItemList()
            {
                Items = new List<Item>(),
                ShippingAddress = new ShippingAddress
                {
                    RecipientName = order.CustomerInfo.Name,
                    Line1 = order.CustomerInfo.Address,
                    City = "Ho Chi Minh City",
                    Phone = order.CustomerInfo.Phonenumber,
                    CountryCode = "VN",
                }
            };

            double price = 0;
            double totalPrice = 0;

            foreach (var item in order.Items)
            {
                price = Math.Round(((item.ItemTotalPrice * (100 - order.DiscountPercent) / 100) / _VNtoUSD), 2);
                totalPrice += price * item.Quantity;
                itemList.Items.Add(new Item()
                {
                    Name = item.Name,
                    Currency = "USD",
                    Price = price.ToString(),
                    Quantity = item.Quantity.ToString(),
                });
            }

            var payment = new Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = totalPrice.ToString(),
                            Currency = "USD",
                        },
                        ItemList = itemList,
                        Description = "Don hang" + order.sId,
                        InvoiceNumber = order.sId,
                    }
                },
                RedirectUrls = new RedirectUrls()
                {
                    CancelUrl = "http://localhost:8080/#/payPalTY/" + order.sId,
                    ReturnUrl = "http://localhost:8080/#/payPalTY/" + order.sId,
                },

                Payer = new Payer()
                {
                    PaymentMethod = "paypal"
                },
            };

            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(payment);

            /*try
            {*/
            BraintreeHttp.HttpResponse response = await client.Execute(request);
            var statusCode = response.StatusCode;
            Payment result = response.Result<Payment>();

            var links = result.Links.GetEnumerator();
            string url = null;
            while (links.MoveNext())
            {
                LinkDescriptionObject lnk = links.Current;
                if (lnk.Rel.ToLower().Trim().Equals("approval_url"))
                {
                    url = lnk.Href;
                    return new PayPalObject
                    {
                        Url = url,
                        Id = result.Id,
                    };
                }
            }

            return null;
        }

        public async Task ApprovePayPal(string id, string Pid)
        {
            var environment = new SandboxEnvironment(_clientId, _secretKey);
            var client = new PayPalHttpClient(environment);

            PaymentExecuteRequest request = new PaymentExecuteRequest(id);
            var paymentEx = new PaymentExecution
            {
                PayerId = Pid,
            };

            request.RequestBody(paymentEx);

            BraintreeHttp.HttpResponse response = await client.Execute(request);

        }

        public async Task<PayPalObject> GetPayPalDetail(string id)
        {
            var environment = new SandboxEnvironment(_clientId, _secretKey);
            var client = new PayPalHttpClient(environment);

            PaymentGetRequest request = new PaymentGetRequest(id);

            /*try
            {*/
            BraintreeHttp.HttpResponse response = await client.Execute(request);

            Console.WriteLine(response);
            var statusCode = response.StatusCode;
            Payment result = response.Result<Payment>();

            return new PayPalObject
            {
                Id = result.Id,
                CreatedTime = result.CreateTime,
                State = result.State,
                Amount = result.Transactions[0].Amount.Total,
                Payer = result.Payer.PayerInfo.PayerId
            };
        }
    }
}
