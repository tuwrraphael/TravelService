using System;
using System.Net.Http;
using System.Threading.Tasks;
using TravelService.Client.ApiDefinition;

namespace TravelService.Client.Impl
{
    internal class LocationsApi : ILocationsApi
    {
        private readonly Func<Task<HttpClient>> clientFactory;
        private readonly string _userId;

        public LocationsApi(Func<Task<HttpClient>> clientFactory, string userId)
        {
            this.clientFactory = clientFactory;
            _userId = userId;
        }

        public ILocationApi this[string name] => new LocationApi(clientFactory, _userId, name);
    }
}