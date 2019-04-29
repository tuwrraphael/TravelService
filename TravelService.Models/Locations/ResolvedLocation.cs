using System.Collections.Generic;

namespace TravelService.Models.Locations
{
    public class ResolvedLocation
    {
        public ResolvedLocation(Coordinate coordinate)
        {
            Coordinate = coordinate;
            Attributes = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Attributes { get; set; }
        public Coordinate Coordinate { get; set; }
        public string Address { get; set; }
    }
}
