using CalendarService.Client;
using DigitService.Client;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService
{
    public class LocationProvider : ILocationProvider
    {
        private readonly ICalendarServiceClient client;
        private readonly IGeocodeProvider geocodeProvider;
        private readonly IDigitServiceClient digitServiceClient;

        public LocationProvider(ICalendarServiceClient client, IGeocodeProvider geocodeProvider,
            IDigitServiceClient digitServiceClient)
        {
            this.client = client;
            this.geocodeProvider = geocodeProvider;
            this.digitServiceClient = digitServiceClient;
        }

        public async Task<UserLocation> GetUserLocationAsync(string userId)
        {
            var evt = await client.GetCurrentEventAsync(userId);
            if (null != evt)
            {
                return new UserLocation(evt.Location);
            }
            var loc = await digitServiceClient.Location[userId].GetAsync();
            if (null != loc)
            {
                return new UserLocation(new Coordinate()
                {
                    Lng = loc.Longitude,
                    Lat = loc.Latitude
                });
            }
            throw new UserLocationNotFoundException();
        }
    }
}