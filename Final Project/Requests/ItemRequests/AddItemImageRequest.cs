using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ItemRequests
{
    public class AddItemImageRequest
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public IFormFile? Image { get; set; }

    }
}
