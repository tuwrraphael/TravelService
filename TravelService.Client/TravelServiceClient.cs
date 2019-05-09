using Microsoft.Extensions.Options;
using OAuthApiClient.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;
using TravelService.Client.ApiDefinition;
using TravelService.Client.Impl;

namespace TravelService.Client
{
    public partial class TravelServiceClient : ITravelServiceClient
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

        private async Task<HttpClient> GetClientAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = options.TravelServiceBaseUri
            };
            await authenticationProvider.AuthenticateClient(client);
            return client;
        }
    }
}
