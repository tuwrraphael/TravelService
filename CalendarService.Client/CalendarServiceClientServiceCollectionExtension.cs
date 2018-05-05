using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OAuthApiClient;

namespace CalendarService.Client
{
    public static class CalendarServiceClientServiceCollectionExtension
    {
        public static void AddCalendarServiceClient(this IServiceCollection services)
        {
            services.AddTransient<ITokenStore, MemoryCacheTokenStore>();
            services.AddTransient<IAuthenticationProvider, ClientCredentialsAuthenticationProvider>();
            services.AddTransient<ICalendarServiceClient>(
                svc =>
                {
                    var optionsAccessor = svc.GetService<IOptions<CalendarServiceOptions>>();
                    var options = optionsAccessor.Value;
                    return new CalendarServiceClient(
                   new ClientCredentialsAuthenticationProvider(svc.GetService<ITokenStore>(),
                   new AuthenticationProviderConfig()
                   {
                       ClientId = options.CalendarServiceClientId,
                       ClientSecret = options.CalendarServiceClientSecret,
                       Name = "c32d88ae-e814-4f41-8247-edab89ac87cb",
                       Scopes = "calendar.service",
                       ServiceIdentityBaseUrl = options.ServiceIdentityUrl,
                   }),
                   optionsAccessor);
                });
        }
    }
}
