using System;
using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class TraceLocation
    {
        public Coordinate Coordinate { get; set; }
        /// <summary>
        /// radius of confidence of the coordinate
        /// </summary>

        public TraceLocationAccuracy Accuracy { get; set; }
        
        public DateTimeOffset Timestamp { get; set; }
    }
}
