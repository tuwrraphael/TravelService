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
            var evt = await client.Users[userId].Events.GetCurrentAsync();
            if (null != evt)
            {
                if (evt.Location.Coordinate != null)
                {
                    return new UserLocation(new Coordinate()
                    {
                        Lng = evt.Location.Coordinate.Longitude,
                        Lat = evt.Location.Coordinate.Latitude
                    });
                }
                if (evt.Location.Address != null)
                {
                    return new UserLocation($"{evt.Location.Address.Street} {evt.Location.Address.PostalCode} {evt.Location.Address.City} {evt.Location.Address.CountryOrRegion}");
                }
                return new UserLocation(evt.Location.Text);
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