using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.RoleRequests
{
    public class AddRoleRequest
    {
        [Required(ErrorMessage = "The name field is required")]
        public string? Name { get; set; }
    }
}
