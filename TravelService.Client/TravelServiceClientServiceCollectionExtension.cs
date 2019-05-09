using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OAuthApiClient.Abstractions;
using System;
using TravelService.Client.ApiDefinition;

namespace TravelService.Client
{
    public static class TravelServiceClientServiceCollectionExtension
    {
        public static void AddTravelServiceClient(this IServiceCollection services,
            Uri baseUri,
            IAuthenticationProviderBuilder authenticationProviderBuilder)
        {
            var factory = authenticationProviderBuilder.GetFactory();
            services.Configure<TravelServiceOptions>(v => v.TravelServiceBaseUri = baseUri);
            services.AddTransient<ITravelServiceClient>(v => new TravelServiceClient(factory(v), v.GetService<IOptions<TravelServiceOptions>>()));
        }
    }
}
