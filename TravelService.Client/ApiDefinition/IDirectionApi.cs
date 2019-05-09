﻿using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client
{
    public interface IDirectionApi
    {
        Task<DirectionsResult> GetAsync();
        IItinerariesApi Itineraries { get; }
    }

    public interface IItinerariesApi
    {
        IItineraryApi this[int index] { get; }
    }

    public interface IItineraryApi
    {
        Task<TraceMeasures> Trace(TraceLocation location);
    }
}