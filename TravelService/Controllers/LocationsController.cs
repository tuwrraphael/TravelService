using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Models;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Controllers
{
    [Route("api/")]
    public class LocationsConroller : Controller
    {
        private readonly ILocationsService locationsService;

        public LocationsConroller(ILocationsService locationsService)
        {
            this.locationsService = locationsService;
        }

        private async Task<IActionResult> ResolveLocations(string term, string userId, UserLocation userLocation)
        {
            var location = await locationsService.ResolveAsync(term, userId, userLocation);
            return Ok(location);
        }

        [HttpGet("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Get(string term, [FromQuery]double? lat, [FromQuery]double? lng)
        {
            return await ResolveLocations(term, User.GetId(), lat.HasValue && lng.HasValue ? new UserLocation(new Coordinate()
            {
                Lat = lat.Value,
                Lng = lng.Value
            }) : null);
        }

        [HttpPut("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Put(string term, [FromBody] ResolvedLocation resolvedLocation)
        {
            await locationsService.PersistAsync(term, User.GetId(), resolvedLocation);
            return Ok();
        }

        [HttpDelete("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Delete(string term)
        {
            await locationsService.DeleteAsync(term, User.GetId());
            return Ok();
        }
    }
}
