using Final_Project.Utils.Resources.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class UpdateProfileRequest
    {
        [Required]
        public string? Id { get; init; }
        [Required]
        public string? Fullname { get; set; }
        public List<String>? Addresses { get; set; }
        public int? Gender { get; set; }
        [DateOfBirth(MinAge = 12, MaxAge = 100)]
        public long? DoB { get; set; }
        [Required]
        public string? Ranking { get; set; }
        [Required]
        public long? Point { get; set; }
        [Required]
        public string? RoleId { get; set; }
    }
}
