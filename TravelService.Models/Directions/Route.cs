using System;

namespace TravelService.Models.Directions
{
    public class Route
    {
        public DateTimeOffset DepatureTime { get; set; }
        public DateTimeOffset ArrivalTime { get; set; }
        /// <summary>
        /// duration in seconds
        /// </summary>
        public double? Duration { get; set; }
        public string StartAddress { get; set; }
        public Coordinate StartLocation { get; set; }
        public string EndAddress { get; set; }
        public Coordinate EndLocation { get; set; }
        public Step[] Steps { get; set; }
    }
}
