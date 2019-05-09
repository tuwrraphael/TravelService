using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class TraceLocationAccuracy
    {
        [Required]
        public double Radius { get; set; }
        [Required]
        public double Confidence { get; set; }
    }
}
