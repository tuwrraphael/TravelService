using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl.WienerLinien.RoutingClient
{
    public class WienerLinienRoutingClient : IWienerLinienRoutingClient
    {
        private readonly HttpClient _client;

        public WienerLinienRoutingClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<WLRoutingResponse> RequestTripAsync(TransitDirectionsRequest directionsRequest)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["language"] = "de";
            query["outputFormat"] = "json";
            query["type_origin"] = "coord";
            query["name_origin"] = $"{directionsRequest.From.Lng.ToString(CultureInfo.InvariantCulture)}:{directionsRequest.From.Lat.ToString(CultureInfo.InvariantCulture)}:WGS84";
            query["type_destination"] = "coord";
            query["name_destination"] = $"{directionsRequest.To.Lng.ToString(CultureInfo.InvariantCulture)}:{directionsRequest.To.Lat.ToString(CultureInfo.InvariantCulture)}:WGS84";

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            var viennaTime = directionsRequest.DateTime.ToOffset(timeZoneInfo.GetUtcOffset(directionsRequest.DateTime));
            query["itdTripDateTimeDepArr"] = directionsRequest.ArriveBy ? "arr" : "dep";
            query["itdDate"] = viennaTime.DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            query["itdTime"] = viennaTime.DateTime.ToString("HHmm");
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
