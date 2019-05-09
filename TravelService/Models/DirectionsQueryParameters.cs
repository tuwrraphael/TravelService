using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace TravelService.Models
{
    public class DirectionsQueryParameters
    {
        public string StartAddress { get; set; }
        public double? StartLat { get; set; }
        public double? StartLng { get; set; }
        [Required]
        public string EndAddress { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset? DepartureTime { get; set; }
    }

    public class TraceQueryParameters
    {
        [Required]
        public double Lng { get; set; }
        [Required]
        public double Lat { get; set; }
        [Required]
        public double AccuracyRadius { get; set; }
        [Required]
        public double AccuracyConfidence { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }
    }
}
