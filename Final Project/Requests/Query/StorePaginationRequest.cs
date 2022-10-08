using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class StorePaginationRequest
    {       
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }       
    }
}

