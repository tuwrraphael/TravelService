using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface IDirectionService
    {
        Task<DirectionsResult> GetTransitAsync(DirectionsRequest request);
    }
}