using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Client.ApiDefinition;
using TravelService.Models.Directions;

namespace TravelService.Client.Impl
{
    internal class DirectionApi : IDirectionApi
    {
        private readonly Func<Task<HttpClient>> clientFactory;
        private readonly string key;

        public DirectionApi(Func<Task<HttpClient>> clientFactory, string key)
        {
            this.clientFactory = clientFactory;
            this.key = key;
        }

        public IItinerariesApi Itineraries => new ItinerariesApi(clientFactory, key);

        public async Task<DirectionsResult> GetAsync()
        {
            var res = await (await clientFactory()).GetAsync($"api/directions/{key}");
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
                return null;
            }
            throw new TravelServiceException($"Could not get transit directions: {res.StatusCode}");
        }

        public async Task<string> Subscribe(Uri callback)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["callback"] = callback.ToString();
            var res = await (await clientFactory()).PostAsync($"api/directions/{key}/subscribe?{query.ToString()}", null);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsStringAsync();
            }
            else
            {
                throw new TravelServiceException($"Could not subscribe: {res.StatusCode}");
            }
        }
    }
}