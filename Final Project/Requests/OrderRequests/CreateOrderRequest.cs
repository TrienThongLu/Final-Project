using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.OderRequests
{
    public class CreateOrderRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public long TotalPrice { get; set; }
        public string? Note { get; set; }
        [Required]
        public List<orderItem> Items { get; set; }
    }
    public record orderItem(string id,string GroupSize,List<string> Topping );

}
