using System;
using TravelService.Models.Locations;

namespace TravelService.Models
{
    public class DirectionsRequest
    {
        public UserLocation StartAddress { get; set; }
        public ResolvedLocation EndAddress { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset? DepartureTime { get; set; }
    }
}
