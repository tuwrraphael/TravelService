using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl
{
    public class LocationsService : ILocationsService
    {
        private readonly IResolvedLocationsStore resolvedLocationsStore;
        private readonly ILocationsProvider locationsProvider;

        public LocationsService(IResolvedLocationsStore resolvedLocationsStore,
            ILocationsProvider locationsProvider)
        {
            this.resolvedLocationsStore = resolvedLocationsStore;
            this.locationsProvider = locationsProvider;
        }

        public async Task DeleteAsync(string userId, string term)
        {
            await resolvedLocationsStore.DeleteAsync(term, userId);
        }

        public async Task PersistAsync(string userId, string term, ResolvedLocation resolvedLocation)
        {
            await resolvedLocationsStore.PersistAsync(term, userId, resolvedLocation);
        }

        public async Task<ResolvedLocation> ResolveAnonymousAsync(UnresolvedLocation toResolve, ResolvedLocation userLocation = null)
        {
            if (null != toResolve.Coordinate)
            {
                return new ResolvedLocation(toResolve.Coordinate);
            }
            if (null == toResolve.Address)
            {
                return null;
            }
            return (await locationsProvider.Find(toResolve.Address, userLocation))?.FirstOrDefault();
        }

        public async Task<ResolvedLocation> ResolveAsync(string userId, UnresolvedLocation toResolve, ResolvedLocation userLocation = null)
        {
            if (null == toResolve.Address)
            {
                return null;
            }
            if (null != toResolve.Coordinate)
            {
                return new ResolvedLocation(toResolve.Coordinate);
            }
            var resolved = await resolvedLocationsStore.GetAsync(toResolve.Address, userId);
            if (null != resolved)
            {
                return resolved;
            }
            return (await locationsProvider.Find(toResolve.Address, userLocation))?.FirstOrDefault();
        }
    }
}
