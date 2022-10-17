using Final_Project.Models;
using System.ComponentModel.DataAnnotations;
using static Final_Project.Models.OrderModel;

namespace Final_Project.Requests.OrderRequests
{
    public class CreateOrderRequest
    {
        public string StoreId { get; set; }
        public List<OrderItem> Items { get; set; }
        public long TotalPrice { get; set; }
        public int DiscountPercent { get; set; }
        public long Amount { get; set; }
        public string TakenBy { get; set; }
        public UserDetail? CustomerInfo { get; set; }
        public int Type { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentDetail? PaymentInfo { get; set; }
        public string Note { get; set; }
    }
    public record OrderItem(string Name, long BasePrice, long Price, long ItemTotalPrice, int Quantity, string Size, List<Topping> Topping );
    public record Topping (string Name, int Quantity, long Price);
}
