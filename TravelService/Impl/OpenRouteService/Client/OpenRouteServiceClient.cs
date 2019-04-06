using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl
{
    public class OpenRouteServiceClient : IOpenRouteServiceClient
    {
        private readonly ApiOptions options;
        private readonly HttpClient _httpClient;

        public OpenRouteServiceClient(HttpClient httpClient, IOptions<ApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
            _httpClient = httpClient;
        }

        public async Task<OpenRouteGeocodeResult> Geocode(string term, Coordinate coord)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api_key"] = options.OpenRouteApiKey;
            query["text"] = term;
            query["lang"] = "de";
            if (null != coord)
            {
                query["focus.point.lon"] = coord.Lng.ToString(CultureInfo.InvariantCulture);
                query["focus.point.lat"] = coord.Lat.ToString(CultureInfo.InvariantCulture);
            }
            var res = await _httpClient.GetAsync($"/geocode/search?{query}");
            if (!res.IsSuccessStatusCode)
            {
                throw new OpenRouteServiceException($"{res.StatusCode}");
            }
            return JsonConvert.DeserializeObject<OpenRouteGeocodeResult>(await res.Content.ReadAsStringAsync());
        }
    }
}
