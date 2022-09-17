using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ItemTypeRequest
{
    public class UpdateTypeRequest
    {
        [Required]
        public string TypeId { get; set; }
        [Required(ErrorMessage = "The name field is required")]
        public string? Name { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }
}
