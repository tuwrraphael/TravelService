using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl
{
    public interface IWienerLinienRoutingClient
    {
        Task<WLRoutingResponse> RequestTripAsync(DirectionsRequest directionsRequest);
    }
}