using System;

namespace TravelService.Models
{
    public class Plan
    {
        public Itinerary[] Itineraries { get; set; }
        public Place From { get; set; }
        public Place To { get; set; }
    }

    public class Itinerary
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Leg[] Legs { get; set; }
    }

    public class Place
    {
        public string Name { get; set; }
        public Coordinate Coordinate { get; set; }
    }

    public class Leg
    {
        public bool TransitLeg { get; set; }
        public Place From { get; set; }
        public Place To { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Line Line { get; set; }
        public int NumStops { get; set; }
        public Coordinate[] Geometry { get; set; }
        public string Headsign { get; set; }
    }

    public class Line
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string VehicleType { get; set; }
    }
}
