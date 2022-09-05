using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class RemoveAddressRequest
    {
        [Required]
        public string? Id { get; init; }
        [Required]
        public string? Address { get; set; }
    }
}
