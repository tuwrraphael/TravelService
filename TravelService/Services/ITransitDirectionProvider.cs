using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface ITransitDirectionProvider
    {
        Task<TransitDirections> GetDirectionsAsync(DirectionsRequest request);
    }
}