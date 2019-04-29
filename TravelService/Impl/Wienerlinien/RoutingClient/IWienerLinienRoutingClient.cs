using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl.WienerLinien.RoutingClient
{
    public interface IWienerLinienRoutingClient
    {
        Task<WLRoutingResponse> RequestTripAsync(TransitDirectionsRequest directionsRequest);
    }
}