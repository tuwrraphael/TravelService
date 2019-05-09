using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Client.ApiDefinition;
using TravelService.Models;

namespace TravelService.Client.Impl
{
    internal class ItineraryApi : IItineraryApi
    {
        private readonly Func<Task<HttpClient>> _clientFactory;
        private readonly string _directionsKey;
        private readonly int _index;

        public ItineraryApi(Func<Task<HttpClient>> clientFactory, string directionsKey, int index)
        {
            _clientFactory = clientFactory;
            _directionsKey = directionsKey;
            _index = index;
        }

        public async Task<TraceMeasures> Trace(TraceLocation location)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["lat"] = location.Coordinate.Lat.ToString(CultureInfo.InvariantCulture);
            query["lng"] = location.Coordinate.Lng.ToString(CultureInfo.InvariantCulture);
            query["accuracyRadius"] = location.Accuracy.Radius.ToString(CultureInfo.InvariantCulture);
            query["accuracyConfidence"] = location.Accuracy.Confidence.ToString(CultureInfo.InvariantCulture);
            query["timeStamp"] = location.Timestamp.ToString("o");
            var url = $"api/directions/{_directionsKey}/itinerarys/{_index}/trace?{query.ToString()}";
            var res = await (await _clientFactory()).GetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                var traceMeasures = JsonConvert.DeserializeObject<TraceMeasures>(content);
                return traceMeasures;
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            throw new TravelServiceException($"Could not trace: {res.StatusCode}");
        }
    }
}