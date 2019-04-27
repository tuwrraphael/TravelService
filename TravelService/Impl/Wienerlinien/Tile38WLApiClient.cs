using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl.WienerLinien
{
    public class Tile38WLApiClient : ITile38WLApiClient
    {
        private readonly HttpClient _client;

        public Tile38WLApiClient (HttpClient client)
        {
            _client = client;
        }

        public async Task<Station[]> GetNearbyStations(Coordinate coordinate, double distance, int limit)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["limit"] = limit.ToString();
            query["distance"] = distance.ToString(CultureInfo.InvariantCulture);
            query["lat"] = coordinate.Lat.ToString(CultureInfo.InvariantCulture);
            query["lng"] = coordinate.Lng.ToString(CultureInfo.InvariantCulture);
            var res = await _client.GetAsync($"haltestellen?{query.ToString()}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Request to Tile38WL returned : {res.StatusCode}");
            }
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Station[]>(content);
        }
    }
}

