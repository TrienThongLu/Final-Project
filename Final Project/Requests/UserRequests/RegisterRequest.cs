using Final_Project.Utils.Resources.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Fullname field is required")]
        public string FullName { get; set; }
        [Required]
/*        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"[0](1[0-1]|3[2-9]|5[6|8|9]|7[0|6-9]|8[0-6|8|9]|9[0-4|6-9])[0-9]{7}", ErrorMessage = "Please enter valid phone no.")]*/
        [PhoneNumber]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string? Password { get; set; }
    }
}
