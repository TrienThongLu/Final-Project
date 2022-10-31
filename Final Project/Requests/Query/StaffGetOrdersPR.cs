using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class StaffGetOrdersPR
    {
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "type")]
        public string? type { get; set; }
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }
        [FromQuery(Name = "date")]
        public string? date { get; set; }
    }
}
