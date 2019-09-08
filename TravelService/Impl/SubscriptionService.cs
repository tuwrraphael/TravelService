using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TravelService.Impl.EF;
using TravelService.Impl.OpenTripPlanner;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IOpenTripPlannerClient _openTripPlannerClient;
        private readonly TravelServiceContext _travelServiceContext;
        private readonly IDirectionsCache _directionsCache;
        private readonly TravelServiceOptions _options;

        public SubscriptionService(IOpenTripPlannerClient openTripPlannerClient,
            TravelServiceContext travelServiceContext,
            IOptions<TravelServiceOptions> optionsAccessor,
            IDirectionsCache directionsCache)
        {
            _openTripPlannerClient = openTripPlannerClient;
            _travelServiceContext = travelServiceContext;
            _directionsCache = directionsCache;
            _options = optionsAccessor.Value;
        }

        public async Task OTPCallback(string subscriptionId, OpenTripPlannerResponse route)
        {
            var sub = await _travelServiceContext.Subscriptions.FindAsync(subscriptionId);
            if (null == sub)
            {
                return;
            }
            route.Id = sub.RouteId;
            await _directionsCache.PutAsync(sub.RouteId, route.ToPlan());
            await new HttpClient().PostAsync(sub.Callback, new StringContent(
                JsonConvert.SerializeObject(new DirectionsUpdate()
                {
                    Id = sub.RouteId
                }), Encoding.UTF8, "application/json"));
        }

        public async Task<string> Subscribe(string routeId, string callback)
        {
            var id = Guid.NewGuid().ToString();
            await _openTripPlannerClient.Subscribe(routeId, new Uri(_options.CallbackUri, $"api/callback/otp/{id}"));
            await _travelServiceContext.AddAsync(new Subscription()
            {
                Id = id,
                RouteId = routeId,
                Callback = callback
            });
            await _travelServiceContext.SaveChangesAsync();
            return id;
        }
    }
}
