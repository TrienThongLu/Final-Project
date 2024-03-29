﻿using Final_Project.Utils.Resources.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.UserRequests
{
    public class UpdateProfileRequest
    {
        [Required]
        public string? Id { get; init; }
        [Required]
        [MinLength(5, ErrorMessage = "The name's length must better than 5 or less than 30")]
        [MaxLength(30, ErrorMessage = "The name's length must better than 5 or less than 30")]
        public string? Fullname { get; set; }
        public int? Gender { get; set; }
        [DateOfBirth(MinAge = 12, MaxAge = 100)]
        public long? DoB { get; set; }
    }
}
