using TravelService.Models;

namespace TravelService.Services
{
    public interface IUserRouteTracer
    {
        TraceMeasures TraceUserOnItinerary(Itinerary itinerary, TraceLocation location);
        TraceMeasures TraceUserWithParticles(Itinerary itinerary, TraceLocation location);
    }
}
