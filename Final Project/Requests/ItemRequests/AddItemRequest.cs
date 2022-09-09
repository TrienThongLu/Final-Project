using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.Itemrequests
{
    public class AddItemRequest
    {      
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public long? Price { get; set; }
        [Required]
        public string? TypeId { get; set; }
        [Required]
        public IFormFile? Image { get; set; }
    }
}
