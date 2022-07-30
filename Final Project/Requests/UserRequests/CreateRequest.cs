using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class CreateRequest
    {
        [Required(ErrorMessage = "Fullname field is required")]
        public string FullName { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"[0]([35789]|(28))[0-9]{8}", ErrorMessage = "Please enter valid phone no.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "RoleId field is required")]
        public string? RoleId { get; set; }
    }
}
