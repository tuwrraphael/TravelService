using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;

namespace TravelService.Services
{
    public interface ILocationsProvider
    {
        Task<ResolvedLocation[]> Find(string term, ResolvedLocation userLocation);
    }
}