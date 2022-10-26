using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class AdminGetOrdersPR
    {
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "type")]
        public string? type { get; set; }
        [FromQuery(Name = "storeId")]
        public string? storeId { get; set; }
        [FromQuery(Name = "date")]
        public string? date { get; set; }
    }
}
