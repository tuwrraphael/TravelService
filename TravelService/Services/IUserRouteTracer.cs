using TravelService.Models;

namespace TravelService.Services
{
    public interface IUserRouteTracer
    {
        TraceMeasures TraceUserOnItinerary(Itinerary itinerary, UserLocation location);
    }
}
