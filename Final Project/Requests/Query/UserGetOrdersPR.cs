using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class UserGetOrdersPR
    {
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "date")]
        public long? date { get; set; }
    }
}
