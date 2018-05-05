using System;

namespace TravelService.Models
{
    public class LocationProviderConfiguration
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public Coordinate StaticLocation { get; set; }
    }

    public class StaticLocationProviderConfiguration
    {
        public Coordinate Location { get; set; }
        public int StartHourUtc { get; set; }
        public int StartMinuteUtc { get; set; }
        public int Duration { get; set; }
        public int DayOfWeek { get; set; }
        public string Id { get; set; }
    }

    public class ExactLocationProviderConfiguration
    {
        public string Service { get; set; }
        public string Id { get; set; }
    }
 
}
