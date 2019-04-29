using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OAuthApiClient.Abstractions;
using System;
using System.Collections.Specialized;
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

        public IDirectionsApi Directions => new DirectionsApi(GetClientAsync, null);

        public IUsers Users => new UsersApi(GetClientAsync);

        private class UsersApi : IUsers
        {
            private readonly Func<Task<HttpClient>> _clientFactory;

            public UsersApi(Func<Task<HttpClient>> clientFactory)
            {
                _clientFactory = clientFactory;
            }

            public IUserApi this[string userId] => new UserApi(_clientFactory, userId);

            public IUserApi Me => new UserApi(_clientFactory, "me");
        }

        private class UserApi : IUserApi
        {
            private readonly Func<Task<HttpClient>> _clientFactory;
            private readonly string _userId;

            public UserApi(Func<Task<HttpClient>> clientFactory, string userId)
            {
                _clientFactory = clientFactory;
                _userId = userId;
            }
            public IUserDirectionApi Directions => new DirectionsApi(_clientFactory, _userId);
        }

        private async Task<HttpClient> GetClientAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = options.TravelServiceBaseUri
            };
            await authenticationProvider.AuthenticateClient(client);
            return client;
        }

        private class DirectionsApi : IDirectionsApi, IUserDirectionApi
        {
            private readonly Func<Task<HttpClient>> clientFactory;
            private readonly string _userId;

            public DirectionsApi(Func<Task<HttpClient>> clientFactory, string userId)
            {
                this.clientFactory = clientFactory;
                _userId = userId;
            }

            public IDirectionApi this[string cacheKey] => new DirectionApi(clientFactory, cacheKey);

            public ITransitApi Transit => new TransitApi(clientFactory, _userId);
        }

        private class DirectionApi : IDirectionApi
        {
            private readonly Func<Task<HttpClient>> clientFactory;
            private readonly string key;

            public DirectionApi(Func<Task<HttpClient>> clientFactory, string key)
            {
                this.clientFactory = clientFactory;
                this.key = key;
            }
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
        }

        private class TransitApi : ITransitApi
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
}
