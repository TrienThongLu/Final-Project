using Final_Project.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using static Final_Project.Requests.Itemrequests.AddItemRequest;

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
        [Required]
        public List<Sizes> GroupSizes { get; set; }
        public List<string> ToppingIds { get; set; }
        public IFormFile? ImageUpload { get; set; }
    }
}

