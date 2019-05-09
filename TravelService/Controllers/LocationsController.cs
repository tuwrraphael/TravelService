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

        private async Task<IActionResult> ResolveLocations(string term, string userId, ResolvedLocation userLocation)
        {
            var location = await locationsService.ResolveAsync(userId, new UnresolvedLocation(term), userLocation);
            return Ok(location);
        }

        [HttpGet("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Get(string term, [FromQuery]double? lat, [FromQuery]double? lng)
        {
            return await ResolveLocations(term, User.GetId(), lat.HasValue && lng.HasValue ? new ResolvedLocation(new Coordinate()
            {
                Lat = lat.Value,
                Lng = lng.Value
            }) : null);
        }

        [HttpPut("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Put(string term, [FromBody] AddResolvedLocationRequest resolvedLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await locationsService.PersistAsync(User.GetId(), term,
                new ResolvedLocation(new Coordinate(resolvedLocation.Lat, resolvedLocation.Lng)));
            return Ok();
        }

        [HttpPut("{userId}/locations/{term}")]
        [Authorize("Service")]
        public async Task<IActionResult> Put(string userId, string term, [FromBody] AddResolvedLocationRequest resolvedLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await locationsService.PersistAsync(userId, term,
                new ResolvedLocation(new Coordinate(resolvedLocation.Lat, resolvedLocation.Lng)));
            return Ok();
        }

        [HttpDelete("me/locations/{term}")]
        [Authorize("User")]
        public async Task<IActionResult> Delete(string term)
        {
            await locationsService.DeleteAsync(User.GetId(), term);
            return Ok();
        }
    }
}
