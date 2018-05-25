using System;
using System.Collections;
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

        public DirectionService(ILocationProvider locationProvider, IEnumerable<ITransitDirectionProvider> transitDirectionProviders,
            IGeocodeProvider geocodeProvider)
        {
            this.locationProvider = locationProvider;
            this.transitDirectionProviders = transitDirectionProviders;
            this.geocodeProvider = geocodeProvider;
        }

        private async Task<TransitDirections> GetTransitAsync(UserLocation start, string endAddress, DateTime arrivalTime)
        {
            var directionTasks = transitDirectionProviders.Select(v => v.GetDirectionsAsync(start, endAddress, arrivalTime));
            return new TransitDirections() { Routes = (await Task.WhenAll(directionTasks)).Where(v => null != v).SelectMany(v => v.Routes).ToArray() };
        }

        public async Task<TransitDirections> GetTransitAsync(string startAddress, string endAddress, DateTime arrivalTime)
        {
            return await GetTransitAsync(new UserLocation(startAddress), endAddress, arrivalTime);
        }

        public async Task<TransitDirections> GetTransitAsync(Coordinate start, string endAddress, DateTime arrivalTime)
        {
            return await GetTransitAsync(new UserLocation(start), endAddress, arrivalTime);
        }

        public async Task<TransitDirections> GetTransitForUserAsync(string userId, string endAddress, DateTime arrivalTime)
        {
            var start = await locationProvider.GetUserLocationAsync(userId);
            return await GetTransitAsync(start, endAddress, arrivalTime);
        }
    }
}