using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl.GoogleMaps
{
    public class GoogleMapsLocationsProvider : ILocationsProvider
    {
        private readonly ApiOptions options;

        public GoogleMapsLocationsProvider(IOptions<ApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }

        public async Task<ResolvedLocation[]> Find(string term, ResolvedLocation userLocation)
        {
            var request = new GoogleMapsApi.Entities.PlacesText.Request.PlacesTextRequest()
            {
                Query = term,
                ApiKey = options.GoogleMapsApiKey
            };
            if (null != userLocation?.Coordinate)
            {
                request.Location = new GoogleMapsApi.Entities.Common.Location(userLocation.Coordinate.Lat,
                    userLocation.Coordinate.Lng);
            }
            var res = await GoogleMapsApi.GoogleMaps.PlacesText.QueryAsync(request);
            var resWithCooards = res.Results.Where(r => r.Geometry?.Location != null);
            if (!resWithCooards.Any())
            {
                return null;
            }
            return resWithCooards.Select(v => new ResolvedLocation(new Coordinate(v.Geometry.Location.Latitude,
                v.Geometry.Location.Longitude))
            {
                Attributes = new Dictionary<string, string> {
                    {"GoogleMapsPlaceId", v.PlaceId } },
                Address = v.FormattedAddress
            }).ToArray();
        }
    }
}
