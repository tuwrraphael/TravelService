using CalendarService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OAuthApiClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace CalendarService.Client
{
    public class CalendarServiceClient : ICalendarServiceClient
    {
        private readonly IAuthenticationProvider authenticationProvider;
        private readonly CalendarServiceOptions options;

        public CalendarServiceClient(IAuthenticationProvider authenticationProvider, IOptions<CalendarServiceOptions> optionsAccessor)
        {
            this.authenticationProvider = authenticationProvider;
            options = optionsAccessor.Value;
        }

        private async Task<HttpClient> GetClient()
        {
            var client = new HttpClient
            {
                BaseAddress = options.CalendarServiceBaseUri
            };
            await authenticationProvider.AuthenticateClient(client);
            return client;
        }

        public async Task<Event> GetCurrentEvent(string userId)
        {
            var res = await (await GetClient()).GetAsync($"api/calendar/{userId}/current");
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Event>(content);
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            throw new CalendarServiceException($"Could not retrieve current event: {res.StatusCode}");
        }
    }
}
