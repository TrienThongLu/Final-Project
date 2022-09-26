﻿using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Requests.Query
{
    public class ItemPaginationRequest
    {     
        [FromQuery(Name = "currentPage")]
        public int currentPage { get; set; }
        [FromQuery(Name = "searchString")]
        public string? searchString { get; set; }
        [FromQuery(Name = "typeId")]
        public string? typeId { get; set; }
    }
}
