using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl
{
    public class GoogleMapsLocationsProvider : ILocationsProvider
    {
        private readonly GoogleMapsApiOptions options;

        public GoogleMapsLocationsProvider(IOptions<GoogleMapsApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }

        public async Task<ResolvedLocation[]> Find(string term, UserLocation userLocation)
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
            if (!res.Results.Any())
            {
                return null;
            }
            return res.Results.Select(v => new ResolvedLocation()
            {
                Attributes = new Dictionary<string, string> {
                    {"GoogleMapsPlaceId", v.PlaceId } },
                Coordinate = v.Geometry?.Location != null ?
                    new Coordinate()
                    {
                        Lat = v.Geometry.Location.Latitude,
                        Lng = v.Geometry.Location.Longitude
                    } : null,
                Address = v.FormattedAddress
            }).ToArray();
        }
    }
}
