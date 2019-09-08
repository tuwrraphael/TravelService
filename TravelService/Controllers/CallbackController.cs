using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelService.Impl.OpenTripPlanner;
using TravelService.Services;

namespace TravelService.Controllers
{
    [Route("api/[controller]")]
    public class CallbackController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public CallbackController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("otp/{id}")]
        public async Task<IActionResult> Otp(string id, [FromBody]OpenTripPlannerResponse res)
        {
            await _subscriptionService.OTPCallback(id, res);
            return Ok();
        }
    }
}
