using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class UserPaginationRequest
    {
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }
        [FromQuery(Name = "role")]
        public string? role { get; set; }
        [FromQuery(Name = "ranking")]
        public string? ranking { get; set; }
    }
}
