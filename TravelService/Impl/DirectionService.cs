using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Models.Locations;
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

        private async Task<DirectionsResult> GetTransitAsync(UserLocation start, ResolvedLocation endAddress, DateTimeOffset arrivalTime)
        {
            var directionTasks = transitDirectionProviders.Select(v => v.GetDirectionsAsync(start, endAddress, arrivalTime));
            return await directionsCache.PutAsync(new TransitDirections() { Routes = (await Task.WhenAll(directionTasks)).Where(v => null != v).SelectMany(v => v.Routes).ToArray() });
        }

        public async Task<DirectionsResult> GetTransitAsync(string startAddress, string endAddress, DateTimeOffset arrivalTime)
        {
            return await GetTransitAsync(new UserLocation(startAddress), new ResolvedLocation()
            {
                Address = endAddress
            }, arrivalTime);
        }

        public async Task<DirectionsResult> GetTransitAsync(Coordinate start, string endAddress, DateTimeOffset arrivalTime)
        {
            return await GetTransitAsync(new UserLocation(start), new ResolvedLocation()
            {
                Address = endAddress
            }, arrivalTime);
        }

        public async Task<DirectionsResult> GetTransitForUserAsync(string userId, string endAddress, DateTimeOffset arrivalTime)
        {
            var start = await locationProvider.GetUserLocationAsync(userId);
            var resolved = (await locationsService.ResolveAsync(endAddress, userId)) ?? new ResolvedLocation()
            {
                Address = endAddress
            };
            return await GetTransitAsync(start, resolved, arrivalTime);
        }
    }
}