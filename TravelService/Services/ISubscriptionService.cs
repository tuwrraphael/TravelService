using System.Threading.Tasks;
using TravelService.Impl.OpenTripPlanner;

namespace TravelService.Services
{
    public interface ISubscriptionService
    {
        Task OTPCallback(string subscriptionId, OpenTripPlannerResponse route);
        Task<string> Subscribe(string routeId, string callback);
    }
}