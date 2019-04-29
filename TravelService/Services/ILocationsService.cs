using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;

namespace TravelService.Services
{
    public interface ILocationsService
    {
        Task<ResolvedLocation> ResolveAsync(string userId, UnresolvedLocation toResolve, ResolvedLocation biasLocation = null);
        Task<ResolvedLocation> ResolveAnonymousAsync(UnresolvedLocation toResolve, ResolvedLocation biasLocation = null);
        Task PersistAsync(string userId, string term, ResolvedLocation resolvedLocation);
        Task DeleteAsync(string userId, string term);
    }
}
