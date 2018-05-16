﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OAuthApiClient;
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
        private readonly IAuthenticationProvider<ITravelServiceClient> authenticationProvider;
        private readonly TravelServiceOptions options;

        public TravelServiceClient(IAuthenticationProvider<ITravelServiceClient> authenticationProvider, IOptions<TravelServiceOptions> optionsAccessor)
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

            private async Task<TransitDirections> Get(string url)
            {
                var res = await (await clientFactory()).GetAsync(url);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TransitDirections>(content);
                }
                else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw new TravelServiceException($"Could not get transit directions: {res.StatusCode}");
            }

            public async Task<TransitDirections> Get(string endAddress, DateTime arrivalTime, string userId)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["endAddress"] = endAddress;
                query["arrivalTime"] = JsonConvert.SerializeObject(arrivalTime);
                return await Get($"api/{userId}/directions/transit?{query.ToString()}");
            }

            public async Task<TransitDirections> Get(string startAddress, string endAddress, DateTime arrivalTime)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["startAddress"] = startAddress;
                query["endAddress"] = endAddress;
                query["arrivalTime"] = JsonConvert.SerializeObject(arrivalTime);
                return await Get($"api/directions/transit?{query.ToString()}");
            }

            public async Task<TransitDirections> Get(Coordinate startAddress, string endAddress, DateTime arrivalTime)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["startLat"] = startAddress.Lat.ToString(CultureInfo.InvariantCulture);
                query["startLng"] = startAddress.Lng.ToString(CultureInfo.InvariantCulture);
                query["endAddress"] = endAddress;
                query["arrivalTime"] = JsonConvert.SerializeObject(arrivalTime);
                return await Get($"api/directions/transit?{query.ToString()}");
            }
        }
    }
}