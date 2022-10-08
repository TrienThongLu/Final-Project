using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class StorePaginationRequest
    {       
        [FromQuery(Name = "searchName")]
        public string? searchName { get; set; }
        [FromQuery(Name = "SearchAddress")]
        public string? searchAddress { get; set; }
    }
}

