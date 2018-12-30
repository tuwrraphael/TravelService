using System;

namespace TravelService.Models.Directions
{
    public class Step
    {
        /// <summary>
        /// duration in seconds
        /// </summary>
        public double? Duration { get; set; }

        public Stop ArrivalStop { get; set; }
        public DateTimeOffset ArrivalTime { get; set; }
        public Stop DepartureStop { get; set; }
        public DateTimeOffset DepartureTime { get; set; }
        public string Headsign { get; set; }
        public Line Line { get; set; }
        public int NumStops { get; set; }
    }
}
