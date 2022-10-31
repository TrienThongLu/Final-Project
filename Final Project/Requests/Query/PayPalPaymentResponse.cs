namespace Final_Project.Requests.Query
{
    public class PayPalPaymentResponse
    {
        public string orderId { get; set; }
        public string? PPPayId { get; set; }
        public string PPToken { get; set; }
        public string? PPPayer { get; set; }
    }
}
