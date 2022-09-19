using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.GroupSize
{
    public class GroupSizeRequest
    {
        [Required]
        public string Size { get; set; }
    }
}
