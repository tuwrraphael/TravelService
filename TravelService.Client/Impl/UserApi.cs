using System;
using System.Net.Http;
using System.Threading.Tasks;
using TravelService.Client.ApiDefinition;

namespace TravelService.Client.Impl
{
    internal class UserApi : IUserApi
    {
        private readonly Func<Task<HttpClient>> _clientFactory;
        private readonly string _userId;

        public UserApi(Func<Task<HttpClient>> clientFactory, string userId)
        {
            _clientFactory = clientFactory;
            _userId = userId;
        }
        public IUserDirectionApi Directions => new DirectionsApi(_clientFactory, _userId);

        public ILocationsApi Locations => new LocationsApi(_clientFactory, _userId);
    }
}
