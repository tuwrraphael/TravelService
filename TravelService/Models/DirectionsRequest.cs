using System;

namespace TravelService.Models
{
    public class DirectionsRequest
    {
        public string UserId { get; set; }
        public UnresolvedLocation StartAddress { get; set; }
        public UnresolvedLocation EndAddress { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public bool ArriveBy { get; set; }
    }
}
