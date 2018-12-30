using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface IDirectionsCache
    {
        Task<DirectionsResult> PutAsync(TransitDirections directions);
        Task<DirectionsResult> GetAsync(string key);
    }
}