using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;

namespace TravelService.Services
{
    public interface ILocationsService
    {
        Task<ResolvedLocation> ResolveAsync(string term, string userId, UserLocation location = null);
        Task PersistAsync(string term, string userId, ResolvedLocation resolvedLocation);
        Task DeleteAsync(string term, string userId);
    }
}
