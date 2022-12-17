using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.OrderRequests
{
    public class UpdateOrderRequest
    {
        [Required]
        public string OrderId { get; set; }
        public string? Note { get; set; }
        [Required]
        public List<ItemModel> Items { get; set; }
    }
}
