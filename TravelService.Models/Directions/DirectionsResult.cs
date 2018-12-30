using TravelService.Models.Directions;

namespace TravelService.Models.Directions
{
    public class DirectionsResult
    {
        public TransitDirections TransitDirections { get; set; }
        public string CacheKey { get; set; }
    }
}
