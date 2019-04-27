using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl.OpenRouteService.Client
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

        public async Task<TravelTimes> GetMatrix(Coordinate from, TravelTarget[] to)
        {
            var request = new OpenRouteMatrixRequest()
            {
                locations = new[] { from }.Union(to.Select(d => d.Target)).Select(t => new[] { t.Lng, t.Lat }).ToArray(),
                sources = new[] { 0 },
                metrics = new[] { "duration" }
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api_key"] = options.OpenRouteApiKey;
            var res = await _httpClient.PostAsync($"v2/matrix/foot-walking?{query}",
                 new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
            {
                throw new OpenRouteServiceException($"{res.StatusCode}");
            }
            var matrix = JsonConvert.DeserializeObject<OpenRouteMatrixResponse>(await res.Content.ReadAsStringAsync());
            var durations = new Dictionary<TravelTarget, double>();
            for (int i = 0; i < to.Length; i++)
            {
                durations.Add(to[i], matrix.durations[0][i + 1]);
            }
            return new TravelTimes()
            {
                Durations = durations,
                From = from
            };
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


    public class OpenRouteMatrixRequest
    {
        public double[][] locations { get; set; }
        public string[] metrics { get; set; }
        public int[] sources { get; set; }
    }

    public class TravelTimes
    {
        public Coordinate From { get; set; }
        public IDictionary<TravelTarget, double> Durations { get; set; }
    }

    public class TravelTarget
    {
        public Coordinate Target { get; set; }
        public string Id { get; set; }
    }


    public class OpenRouteMatrixResponse
    {
        public float[][] durations { get; set; }
        public Destination[] destinations { get; set; }
    }

    public class Destination
    {
        public float[] location { get; set; }
        public float snapped_distance { get; set; }
    }
}
