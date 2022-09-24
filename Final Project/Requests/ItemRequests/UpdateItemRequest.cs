using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UpdateItemRequests
{
    public class UpdateItemRequests
    {
        [Required]
        public string ItemId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public long? Price { get; set; }
        [Required]
        public string? TypeId { get; set; }
        public IFormFile ImageUpload { get; set; }
    }
}

