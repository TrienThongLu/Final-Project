using Final_Project.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using static Final_Project.Models.ItemModel;

namespace Final_Project.Requests.Itemrequests
{
    public class AddItemRequest
    {      
        [Required]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string TypeId { get; set; }
        [Required]
        public List<Sizes> GroupSizes { get; set; }      
        public List<string> ToppingIds { get; set; }      
        public IFormFile Image { get; set; }

        public record Sizes(string name, long price);
    }    
}
