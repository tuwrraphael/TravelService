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

        public async Task DeleteAsync(string term, string userId)
        {
            await resolvedLocationsStore.DeleteAsync(term, userId);
        }

        public async Task PersistAsync(string term, string userId, ResolvedLocation resolvedLocation)
        {
            await resolvedLocationsStore.PersistAsync(term, userId, resolvedLocation);
        }

        public async Task<ResolvedLocation> ResolveAnonymousAsync(string term, UserLocation userLocation = null)
        {
            return (await locationsProvider.Find(term, userLocation))?.FirstOrDefault() ?? new ResolvedLocation() { Address = term };
        }

        public async Task<ResolvedLocation> ResolveAsync(string term, string userId, UserLocation userLocation = null)
        {
            var resolved = await resolvedLocationsStore.GetAsync(term, userId);
            if (null != resolved)
            {
                return resolved;
            }
            return (await locationsProvider.Find(term, userLocation))?.FirstOrDefault();
        }
    }
}
