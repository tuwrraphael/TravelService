using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Controllers
{
    [Route("api")]
    public class LocationProviderController : Controller
    {
        private readonly ILocationProviderConfigurationService locationProviderService;

        public LocationProviderController(ILocationProviderConfigurationService locationProviderService)
        {
            this.locationProviderService = locationProviderService;
        }


        private async Task<IActionResult> GetProviders(string userId)
        {
            return Ok(await locationProviderService.GetConfigurations(userId));
        }

        private async Task<IActionResult> AddProvider(string userId, LocationProviderConfiguration config)
        {
            return Ok(await locationProviderService.AddProvider(userId, config));
        }

        [Authorize("User")]
        [HttpGet("me/location/providers/static")]
        public async Task<IActionResult> Get()
        {
            return await GetProviders(User.GetId());
        }

        [Authorize("User")]
        [HttpPost("me/location/providers")]
        public async Task<IActionResult> Add([FromBody]LocationProviderConfiguration config)
        {
            return await AddProvider(User.GetId(), config);
        }

        [Authorize("Service")]
        [HttpGet("{userId}/location/providers")]
        public async Task<IActionResult> Get(string userId)
        {
            return await GetProviders(userId);
        }

        [Authorize("Service")]
        [HttpPost("{userId}/location/providers")]
        public async Task<IActionResult> Add([FromBody]LocationProviderConfiguration config, string userId)
        {
            return await AddProvider(userId, config);
        }
    }
}
