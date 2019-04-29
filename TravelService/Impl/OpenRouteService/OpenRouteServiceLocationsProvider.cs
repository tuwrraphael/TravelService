using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Impl.OpenRouteService.Client;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl.OpenRouteService
{
    public class OpenRouteServiceLocationsProvider : ILocationsProvider
    {
        private readonly IOpenRouteServiceClient _openRouteServiceClient;

        public OpenRouteServiceLocationsProvider(IOpenRouteServiceClient openRouteServiceClient)
        {
            _openRouteServiceClient = openRouteServiceClient;
        }
        public async Task<ResolvedLocation[]> Find(string term, ResolvedLocation userLocation)
        {
            var res = await _openRouteServiceClient.Geocode(term, userLocation?.Coordinate);
            if (!res.features.Any())
            {
                return null;
            }
            return res.features.Select(f => new ResolvedLocation(new Coordinate(f.geometry.coordinates[1], f.geometry.coordinates[0]))
            {
                Address = f.properties.label,
                Attributes = new Dictionary<string, string>()
            }).ToArray();
        }
    }
}
