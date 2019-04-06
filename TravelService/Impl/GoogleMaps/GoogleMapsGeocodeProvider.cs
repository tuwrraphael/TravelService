using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl.GoogleMaps
{
    public class GoogleMapsGeocodeProvider : IGeocodeProvider
    {
        private readonly ApiOptions options;

        public GoogleMapsGeocodeProvider(IOptions<ApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }


        public async Task<string> GetAddressAsync(Coordinate start)
        {
            var res = await GoogleMapsApi.GoogleMaps.Geocode.QueryAsync(new GoogleMapsApi.Entities.Geocoding.Request.GeocodingRequest()
            {
                 ApiKey = options.GoogleMapsApiKey,
                 Location = new GoogleMapsApi.Entities.Common.Location(start.Lat, start.Lng)
            });
            if (null != res.Results && res.Results.Any())
            {
                return res.Results.First().FormattedAddress;
            }
            return null;
        }
    }
}