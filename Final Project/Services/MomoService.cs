using System;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Tutorial.Service.MomoService;

namespace Final_Project.Services
{
    public class MomoService
    {
        private readonly IConfiguration _configuration;

        public MomoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<JObject> createMoMoQrRequest(long Amount, string OrderId, string RequestId)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("partnerCode");
            string accessKey = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("accessKey");
            string serectkey = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("serectkey");
            string orderInfo = "pay order: " + OrderId;
            string redirectUrl = "http://localhost:8080/#/thankyou";
            string ipnUrl = "http://localhost:8080/#/thankyou";
            string requestType = "captureWallet";

            string amount = Amount.ToString();
            string orderId = OrderId;
            string requestId = RequestId;
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
                ;

            MomoSecurity crypto = new MomoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };
            try
            {
                string responseFromMomo = MomoRequest.sendPaymentRequest(endpoint, message.ToString());
                //Console.WriteLine(responseFromMomo);
                JObject jmessage = JObject.Parse(responseFromMomo);
                //Console.WriteLine("Return from MoMo: " + jmessage.ToString());

                return jmessage;
            }
            catch
            {
                return null;
            }
        }

        public async Task<JObject> createMoMoATMRequest(long Amount, string OrderId, string RequestId)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("partnerCode");
            string accessKey = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("accessKey");
            string serectkey = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("serectkey");
            string orderInfo = "pay order: " + OrderId;
            string redirectUrl = "http://localhost:8080/#/thankyou";
            string ipnUrl = "http://localhost:8080/#/thankyou";
            string requestType = "payWithATM";

            string amount = Amount.ToString();
            string orderId = OrderId;
            string requestId = RequestId;
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
                ;

            MomoSecurity crypto = new MomoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };
            try
            {
                string responseFromMomo = MomoRequest.sendPaymentRequest(endpoint, message.ToString());
                //Console.WriteLine(responseFromMomo);
                JObject jmessage = JObject.Parse(responseFromMomo);
                //Console.WriteLine("Return from MoMo: " + jmessage.ToString());

                return jmessage;
            }
            catch
            {
                return null;
            }
        }

        public async Task<JObject> queryRequest(string type, string requestId, string orderId)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/query";
            string partnerCode = null;
            string accessKey = null; 
            string secretKey = null;

            if (type == "MoMoQr")
            {
                partnerCode = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("partnerCode");
                accessKey = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("accessKey");
                secretKey = _configuration.GetSection("MoMoPaymentQr").GetValue<string>("serectkey");
            } else if (type == "MoMoATM")
            {
                partnerCode = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("partnerCode");
                accessKey = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("accessKey");
                secretKey = _configuration.GetSection("MoMoPaymentATM").GetValue<string>("serectkey");
            }

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&orderId=" + orderId +
                "&partnerCode=" + partnerCode +
                "&requestId=" + requestId
                ;

            MomoSecurity crypto = new MomoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, secretKey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "requestId", requestId },
                { "orderId", orderId },
                { "lang", "en" },
                { "signature", signature }
            };

            string responseFromMomo = MomoRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return jmessage;
        }
    }
}
