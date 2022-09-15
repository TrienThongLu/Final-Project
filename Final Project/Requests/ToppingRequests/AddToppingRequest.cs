using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ToppingRequests
{
    public class AddToppingRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public long Price { get; set; }
    }
}
