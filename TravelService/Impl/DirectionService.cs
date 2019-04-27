using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Services;

namespace TravelService.Impl
{
    public class DirectionService : IDirectionService
    {
        private readonly ILocationProvider locationProvider;
        private readonly IEnumerable<ITransitDirectionProvider> transitDirectionProviders;
        private readonly IGeocodeProvider geocodeProvider;
        private readonly ILocationsService locationsService;
        private readonly IDirectionsCache directionsCache;

        public DirectionService(ILocationProvider locationProvider, IEnumerable<ITransitDirectionProvider> transitDirectionProviders,
            IGeocodeProvider geocodeProvider,
            ILocationsService locationsService,
            IDirectionsCache directionsCache)
        {
            this.locationProvider = locationProvider;
            this.transitDirectionProviders = transitDirectionProviders;
            this.geocodeProvider = geocodeProvider;
            this.locationsService = locationsService;
            this.directionsCache = directionsCache;
        }

        public async Task<DirectionsResult> GetTransitAsync(DirectionsRequest request)
        {
            //if (request.)
            var directionTasks = transitDirectionProviders.Select(v => v.GetDirectionsAsync(request));
            return await directionsCache.PutAsync(new TransitDirections() { Routes = (await Task.WhenAll(directionTasks)).Where(v => null != v).SelectMany(v => v.Routes).ToArray() });
        }
    }
}