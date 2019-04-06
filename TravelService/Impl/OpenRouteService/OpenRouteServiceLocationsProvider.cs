using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl
{
    public class OpenRouteServiceLocationsProvider : ILocationsProvider
    {
        private readonly IOpenRouteServiceClient _openRouteServiceClient;

        public OpenRouteServiceLocationsProvider(IOpenRouteServiceClient openRouteServiceClient)
        {
            _openRouteServiceClient = openRouteServiceClient;
        }
        public async Task<ResolvedLocation[]> Find(string term, UserLocation userLocation)
        {
            var res = await _openRouteServiceClient.Geocode(term, userLocation?.Coordinate);
            if (!res.features.Any())
            {
                return null;
            }
            return res.features.Select(f => new ResolvedLocation()
            {
                Address = f.properties.label,
                Attributes = new Dictionary<string, string>(),
                Coordinate = new Coordinate()
                {
                    Lat = f.geometry.coordinates[1],
                    Lng = f.geometry.coordinates[0]
                }
            }).ToArray();
        }
    }
}
