using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl.WienerLinien.RoutingClient
{
    public interface IWienerLinienRoutingClient
    {
        Task<WLRoutingResponse> RequestTripAsync(DirectionsRequest directionsRequest);
        Task<WLRoutingResponse> RequestTripAsync(string fromStationId, string toStationId,
            DirectionsRequest directionsRequest);
    }
}