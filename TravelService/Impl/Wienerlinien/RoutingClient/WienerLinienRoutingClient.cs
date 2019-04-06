using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl
{
    public class WienerLinienRoutingClient : IWienerLinienRoutingClient
    {
        private readonly HttpClient _client;

        public WienerLinienRoutingClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<WLRoutingResponse> RequestTripAsync(DirectionsRequest directionsRequest)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["language"] = "de";
            query["outputFormat"] = "json";
            if (null != directionsRequest.StartAddress.Coordinate)
            {
                query["type_origin"] = "coord";
                query["name_origin"] = $"{directionsRequest.StartAddress.Coordinate.Lng.ToString(CultureInfo.InvariantCulture)}:{directionsRequest.StartAddress.Coordinate.Lat.ToString(CultureInfo.InvariantCulture)}:WGS84";
            }
            else
            {
                query["type_origin"] = "any";
                query["name_origin"] = directionsRequest.StartAddress.Address;
            }
            if (null != directionsRequest.EndAddress.Coordinate)
            {
                query["type_destination"] = "coord";
                query["name_destination"] = $"{directionsRequest.EndAddress.Coordinate.Lng.ToString(CultureInfo.InvariantCulture)}:{directionsRequest.EndAddress.Coordinate.Lat.ToString(CultureInfo.InvariantCulture)}:WGS84";
            }
            else
            {
                query["type_destination"] = "any";
                query["name_destination"] = directionsRequest.EndAddress.Address;
            }
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            if (directionsRequest.DepartureTime.HasValue)
            {
                var viennaTime = directionsRequest.DepartureTime.Value.ToOffset(timeZoneInfo.GetUtcOffset(directionsRequest.DepartureTime.Value));
                query["itdTripDateTimeDepArr"] = "dep";
                query["itdDate"] = viennaTime.DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                query["itdTime"] = viennaTime.DateTime.ToString("HHmm");
            }
            else if (directionsRequest.ArrivalTime.HasValue)
            {
                var viennaTime = directionsRequest.ArrivalTime.Value.ToOffset(timeZoneInfo.GetUtcOffset(directionsRequest.ArrivalTime.Value));
                query["itdTripDateTimeDepArr"] = "arr";
                query["itdDate"] = viennaTime.DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                query["itdTime"] = viennaTime.DateTime.ToString("HHmm");
            }
            var res = await _client.GetAsync($"ogd_routing/XML_TRIP_REQUEST2?{query.ToString()}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Request to WienerLinien returned : {res.StatusCode}");
            }
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WLRoutingResponse>(content);
        }
    }
}
