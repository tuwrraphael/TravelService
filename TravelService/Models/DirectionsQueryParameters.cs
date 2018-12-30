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
        [BindRequired]
        public string EndAddress { get; set; }
        [BindRequired]
        public DateTimeOffset ArrivalTime { get; set; }

    }
}
