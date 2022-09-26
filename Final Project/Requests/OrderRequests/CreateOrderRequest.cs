using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.OrderRequests
{
    public class CreateOrderRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public long TotalPrice { get; set; }
        public string? Note { get; set; }
        [Required]
        public List<OrderItem> Items { get; set; }
    }
    public record OrderItem(string Name, int Quantity, string Size,List<Topping> Topping);
    public record Topping (string Name, int Quantity);
}
