using Final_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Requests.StoreLocation
{       
        public class UpdateStoreLocaionRequest
        {
            [Required]
            public string Id { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Address { get; set; }
            [Required]
            public Position Positions { get; set; }
            [Required]
            public int StartTime { get; set; }
            [Required]
            public int CloseTime { get; set; }
    }
}
