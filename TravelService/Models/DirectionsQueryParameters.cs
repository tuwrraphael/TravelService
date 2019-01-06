using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace TravelService.Models
{
    public class DirectionsQueryParameters
    {
        public string StartAddress { get; set; }
        public double? StartLat { get; set; }
        public double? StartLng { get; set; }
        [BindRequired]
        public string EndAddress { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset? DepartureTime { get; set; }
    }
}
