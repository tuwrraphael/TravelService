using System;

namespace TravelService.Models
{
    public class UserLocation
    {
        public Coordinate Coordinate { get; set; }
        /// <summary>
        /// radius of confidence of the coordinate
        /// </summary>
        public UserLocationAccuracy Accuracy { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
    public class UserLocationAccuracy
    {
        public double Radius { get; set; }
        public double Confidence { get; set; }
    }
}
