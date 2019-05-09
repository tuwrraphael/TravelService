using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Client.ApiDefinition;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client.Impl
{
    internal class TransitApi : ITransitApi
    {
        private readonly Func<Task<HttpClient>> clientFactory;
        private readonly string _userId;

        public TransitApi(Func<Task<HttpClient>> clientFactory, string userId)
        {
            this.clientFactory = clientFactory;
            _userId = userId;
        }

        private async Task<DirectionsResult> Get(NameValueCollection queryParams)
        {
            var baseUrl = _userId != null ? $"api/{_userId}/directions/transit" : "api/directions/transit";
            var res = await (await clientFactory()).GetAsync($"{baseUrl}?{queryParams.ToString()}");
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                var directions = JsonConvert.DeserializeObject<TransitDirections>(content);
                var key = res.Headers.ETag.Tag;
                return new DirectionsResult()
                {
                    CacheKey = key?.Replace("\"", string.Empty),
                    TransitDirections = directions
                };
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var content = await res.Content.ReadAsStringAsync();
                var notfound = JsonConvert.DeserializeObject<DirectionsNotFoundResult>(content);
                return new DirectionsResult()
                {
                    NotFound = notfound
                };
            }
            throw new TravelServiceException($"Could not get transit directions: {res.StatusCode}");
        }

        public async Task<DirectionsResult> Get(string startAddress, string endAddress, DateTimeOffset? arrivalTime = null, DateTimeOffset? departureTime = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["startAddress"] = startAddress;
            query["endAddress"] = endAddress;
            if (arrivalTime.HasValue)
            {
                query["arrivalTime"] = arrivalTime.Value.ToString("o");
            }
            if (departureTime.HasValue)
            {
                query["departureTime"] = departureTime.Value.ToString("o");
            }
            return await Get(query);
        }

        public async Task<DirectionsResult> Get(Coordinate startAddress, string endAddress, DateTimeOffset? arrivalTime = null, DateTimeOffset? departureTime = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["startLat"] = startAddress.Lat.ToString(CultureInfo.InvariantCulture);
            query["startLng"] = startAddress.Lng.ToString(CultureInfo.InvariantCulture);
            query["endAddress"] = endAddress;
            if (arrivalTime.HasValue)
            {
                query["arrivalTime"] = arrivalTime.Value.ToString("o");
            }
            if (departureTime.HasValue)
            {
                query["departureTime"] = departureTime.Value.ToString("o");
            }
            return await Get(query);
        }
    }
}