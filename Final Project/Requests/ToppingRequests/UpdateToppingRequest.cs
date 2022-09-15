using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ToppingRequests
{
    public class UpdateToppingRequest
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public long Price { get; set; }
    }
}
