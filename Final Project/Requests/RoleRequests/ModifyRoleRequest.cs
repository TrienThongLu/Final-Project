using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.RoleRequests
{
    public class ModifyRoleRequest
    {
        [Required(ErrorMessage = "The id field is required")]
        public string? Id { get; set; }
        [Required(ErrorMessage = "The name field is required")]
        public string? Name { get; set; }
    }
}
