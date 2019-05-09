using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TravelService.Client
{
    internal class DirectionsApi : IDirectionsApi, IUserDirectionApi
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
}