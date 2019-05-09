using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class AddResolvedLocationRequest
    {
        [Required]
        public double Lat { get; set; }
        [Required]
        public double Lng { get; set; }
    }
}
