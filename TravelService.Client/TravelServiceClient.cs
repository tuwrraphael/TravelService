using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OAuthApiClient.Abstractions;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client
{
    public class TravelServiceClient : ITravelServiceClient
    {
        private readonly IAuthenticationProvider authenticationProvider;
        private readonly TravelServiceOptions options;

        public TravelServiceClient(IAuthenticationProvider authenticationProvider, IOptions<TravelServiceOptions> optionsAccessor)
        {
            this.authenticationProvider = authenticationProvider;
            options = optionsAccessor.Value;
        }

        public IDirectionsApi Directions => new DirectionsApi(GetClientAsync);

        private async Task<HttpClient> GetClientAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = options.TravelServiceBaseUri
            };
            await authenticationProvider.AuthenticateClient(client);
            return client;
        }

        private class DirectionsApi : IDirectionsApi
        {
            private readonly Func<Task<HttpClient>> clientFactory;

            public DirectionsApi(Func<Task<HttpClient>> clientFactory)
            {
                this.clientFactory = clientFactory;
            }

            public ITransitApi Transit => new TransitApi(clientFactory);
        }

        private class TransitApi : ITransitApi
        {
            private readonly Func<Task<HttpClient>> clientFactory;

            public TransitApi(Func<Task<HttpClient>> clientFactory)
            {
                this.clientFactory = clientFactory;
            }

            private async Task<DirectionsResult> Get(string url)
            {
                var res = await (await clientFactory()).GetAsync(url);
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
                return await Get($"api/directions/transit?{query.ToString()}");
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
                return await Get($"api/directions/transit?{query.ToString()}");
            }
        }
    }
}
