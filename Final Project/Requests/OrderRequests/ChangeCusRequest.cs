using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.OrderRequests
{
    public class ChangeCusRequest
    {
        [Required]
        public string OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
