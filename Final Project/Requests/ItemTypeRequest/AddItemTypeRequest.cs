﻿using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.ItemTypeRequest
{
    public class AddItemTypeRequest
    {
        [Required(ErrorMessage = "The name field is required")]
        public string? Name { get; set; }
        public IFormFile Image { get; set; }
    }
}