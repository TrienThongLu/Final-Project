using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class ItemPR
    {     
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }
        [FromQuery(Name = "typeId")]
        public string? typeId { get; set; }
        [FromQuery(Name = "stock")]
        public string? stock { get; set; }
    }
}
