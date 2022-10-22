using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class StorePR
    {       
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }       
    }
}

