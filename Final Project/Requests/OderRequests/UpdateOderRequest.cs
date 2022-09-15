using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.OderRequests
{
    public class UpdateOderRequest
    {
        [Required]
        public string OderId { get; set; }
        public string? Note { get; set; }
        [Required]
        public List<ItemModel> Items { get; set; }
    }
}
