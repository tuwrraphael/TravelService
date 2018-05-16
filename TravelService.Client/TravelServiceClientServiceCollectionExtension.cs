using Microsoft.Extensions.DependencyInjection;
using OAuthApiClient;
using System;

namespace TravelService.Client
{
    public class TravelServiceBuilder
    {
        private readonly IServiceCollection services;

        public TravelServiceBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public void AddClientCredentialsAuthentication(ClientCredentialsConfig clientCredentialsConfig)
        {
            services.AddTransient<IAuthenticationProvider<ITravelServiceClient>>(srv =>
            new BearerTokenAuthenticationProvider<ITravelServiceClient>(srv.GetService<ITokenStore>(), new ClientCredentialsTokenStrategy(clientCredentialsConfig)));
        }
    }

    public static class TravelServiceClientServiceCollectionExtension
    {

        public static TravelServiceBuilder AddCalendarServiceClient(this IServiceCollection services, Uri baseUri)
        {
            services.Configure<TravelServiceOptions>(v => v.TravelServiceBaseUri = baseUri);
            services.AddTransient<ITokenStore, MemoryCacheTokenStore>();
            services.AddTransient<ITravelServiceClient, TravelServiceClient>();
            return new TravelServiceBuilder(services);
        }
    }
}
