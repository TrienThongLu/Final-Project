using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ItemTypeRequest
{
    public class UpdateTypeRequest
    {
        [Required]
        public string TypeId { get; set; }
        [Required]
        public string? Name { get; set; }
        public IFormFile? ImageUpload { get; set; }
        public int Style { get; set; }
    }
}
