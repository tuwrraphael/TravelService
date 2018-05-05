using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl
{
    public class GoogleMapsGeocodeProvider : IGeocodeProvider
    {
        private readonly GoogleMapsApiOptions options;

        public GoogleMapsGeocodeProvider(IOptions<GoogleMapsApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }


        public async Task<string> GetAddressAsync(Coordinate start)
        {
            return "Polgarstraﬂe 32, 1220 Wien";
        }
    }
}