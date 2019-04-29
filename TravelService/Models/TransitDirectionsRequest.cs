using System;

namespace TravelService.Models
{
    public class TransitDirectionsRequest
    {
        public Coordinate From { get; set; }
        public Coordinate To { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public bool ArriveBy { get; set; }
    }
}
