using CalendarService.Client;
using System.Threading.Tasks;
using TravelService.Services;

namespace TravelService
{
    public class LocationProvider : ILocationProvider
    {
        private readonly ICalendarServiceClient client;
        private readonly IGeocodeProvider geocodeProvider;

        public LocationProvider(ICalendarServiceClient client, IGeocodeProvider geocodeProvider)
        {
            this.client = client;
            this.geocodeProvider = geocodeProvider;
        }

        public async Task<string> GetUserLocationAsync(string userId)
        {
            var evt = await client.GetCurrentEventAsync(userId);
            if (null == evt)
            {
                throw new UserLocationNotFoundException();
            }
            return evt.Location;
        }
    }
}