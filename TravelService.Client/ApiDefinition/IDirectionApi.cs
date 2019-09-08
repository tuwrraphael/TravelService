using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client.ApiDefinition
{
    public interface IDirectionApi
    {
        Task<DirectionsResult> GetAsync();
        Task<string> Subscribe(Uri callback);
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