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

        private readonly ITransitDirectionProvider transitDirectionProvider;
        private readonly ILocationsService locationsService;
        private readonly IDirectionsCache directionsCache;

        public DirectionService(ITransitDirectionProvider transitDirectionProvider,
            ILocationsService locationsService,
            IDirectionsCache directionsCache)
        {
            this.transitDirectionProvider = transitDirectionProvider;
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
            if (null == resolvedStart)
            {
                throw new LocationNotFoundException(request.StartAddress);
            }
            if (null == resolvedEnd)
            {
                throw new LocationNotFoundException(request.EndAddress);
            }
            from = resolvedStart.Coordinate;
            to = resolvedEnd.Coordinate;
            var plan = await transitDirectionProvider.GetDirectionsAsync(new TransitDirectionsRequest
            {
                ArriveBy = request.ArriveBy,
                DateTime = request.DateTime,
                From = from,
                To = to
            });
            if (null == plan)
            {
                return null;
            }
            string cacheKey = await directionsCache.PutAsync(plan);
            return new DirectionsResult()
            {
                CacheKey = cacheKey,
                TransitDirections = plan.GetTransitDirections()
            };
        }
    }
}