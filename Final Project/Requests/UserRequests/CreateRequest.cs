using Final_Project.Utils.Resources.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class CreateRequest
    {
        [Required(ErrorMessage = "Fullname field is required")]
        public string FullName { get; set; }
        [Required]
        [PhoneNumber]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "RoleId field is required")]
        public string? RoleId { get; set; }
        public string? StoreId { get; set; }
    }
}
