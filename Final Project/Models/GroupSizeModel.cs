using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class GroupSizeModel
    {
        public GroupSizeModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        public string Id { get; set; }
        public string Size { get; set; } 
    }
}
