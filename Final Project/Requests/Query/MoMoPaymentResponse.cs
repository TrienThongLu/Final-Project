using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class MoMoPaymentResponse
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public long transId { get; set; }
        public int resultCode { get; set; }
    }
}
