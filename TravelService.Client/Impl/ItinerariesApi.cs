using System;
using System.Net.Http;
using System.Threading.Tasks;
using TravelService.Client.ApiDefinition;

namespace TravelService.Client.Impl
{
    internal class ItinerariesApi : IItinerariesApi
    {
        private readonly Func<Task<HttpClient>> _clientFactory;
        private readonly string _directionsKey;

        public ItinerariesApi(Func<Task<HttpClient>> clientFactory, string directionsKey)
        {
            _clientFactory = clientFactory;
            _directionsKey = directionsKey;
        }

        public IItineraryApi this[int index] => new ItineraryApi(_clientFactory, _directionsKey, index);
    }
}