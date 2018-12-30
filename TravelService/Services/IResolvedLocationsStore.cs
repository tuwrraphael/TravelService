using System.Threading.Tasks;
using TravelService.Models.Locations;

namespace TravelService.Services
{
    public interface IResolvedLocationsStore
    {
        Task<ResolvedLocation> GetAsync(string term, string userId);
        Task PersistAsync(string term, string userId, ResolvedLocation resolvedLocation);
        Task DeleteAsync(string term, string userId);
    }
}
