using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class StatisticQuery
    {
        [FromQuery(Name = "from")]
        public string? from { get; set; }
        [FromQuery(Name = "to")]
        public string? to { get; set; }
    }
}
