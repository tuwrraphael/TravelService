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
        private static readonly ResolvedLocation Vienna = new ResolvedLocation(new Coordinate(48.210033, 16.363449));

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
            Coordinate from, to;
            ResolvedLocation resolvedStart, resolvedEnd = null;
            if (request.UserId != null)
            {
                var locationBias = (await locationsService.ResolveAsync(request.UserId, new UnresolvedLocation(UnresolvedLocation.Home))) ?? Vienna;
                resolvedStart = await locationsService.ResolveAsync(request.UserId, request.StartAddress, locationBias);
                if (null != resolvedStart)
                {
                    resolvedEnd = await locationsService.ResolveAsync(request.UserId, request.EndAddress, resolvedStart);
                }

            }
            else
            {
                var locationBias = Vienna;
                resolvedStart = await locationsService.ResolveAnonymousAsync(request.StartAddress, locationBias);
                if (null != resolvedStart)
                {
                    resolvedEnd = await locationsService.ResolveAnonymousAsync(request.EndAddress, resolvedStart);
                }
            }
            if (null != resolvedStart)
            {
                throw new LocationNotFoundException(request.StartAddress);
            }
            if (null != resolvedEnd)
            {
                throw new LocationNotFoundException(request.EndAddress);
            }
            from = resolvedStart.Coordinate;
            to = resolvedEnd.Coordinate;
            var directionTasks = transitDirectionProviders.Select(v => v.GetDirectionsAsync(new TransitDirectionsRequest
            {
                ArriveBy = request.ArriveBy,
                DateTime = request.DateTime,
                From = from,
                To = to
            }));
            return await directionsCache.PutAsync(new TransitDirections() { Routes = (await Task.WhenAll(directionTasks)).Where(v => null != v).SelectMany(v => v.Routes).ToArray() });
        }
    }
}