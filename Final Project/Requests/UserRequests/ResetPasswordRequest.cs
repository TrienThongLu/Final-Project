using Final_Project.Utils.Resources.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class ResetPasswordRequest
    {
        [Required]
        [PhoneNumber]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string? Password { get; set; }
    }
}
