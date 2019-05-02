using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Controllers
{
    [Route("api/")]
    public class DirectionsController : Controller
    {
        private readonly IDirectionService directionsService;
        private readonly IDirectionsCache directionsCache;
        private readonly ILogger<DirectionsController> _logger;

        public DirectionsController(IDirectionService directionsService,
            IDirectionsCache directionsCache, ILogger<DirectionsController> logger)
        {
            this.directionsService = directionsService;
            this.directionsCache = directionsCache;
            _logger = logger;
        }

        [HttpGet("directions/transit")]
        public async Task<IActionResult> Get([FromQuery]DirectionsQueryParameters directionsQueryParameters)
        {
            return await GetDirections(null, directionsQueryParameters);
        }

        private async Task<IActionResult> GetDirections(string userId, DirectionsQueryParameters directionsQueryParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (directionsQueryParameters.ArrivalTime.HasValue == directionsQueryParameters.DepartureTime.HasValue)
            {
                return BadRequest("Either departureTime xor arrialTime must be specified.");
            }
            UnresolvedLocation start;
            if (null != directionsQueryParameters.StartAddress)
            {
                start = new UnresolvedLocation(directionsQueryParameters.StartAddress);

            }
            else if (directionsQueryParameters.StartLat.HasValue && directionsQueryParameters.StartLng.HasValue)
            {
                start = new UnresolvedLocation(new Coordinate()
                {
                    Lat = directionsQueryParameters.StartLat.Value,
                    Lng = directionsQueryParameters.StartLng.Value
                });
            }
            else
            {
                return BadRequest();
            }
            try
            {
                var res = await directionsService.GetTransitAsync(new DirectionsRequest()
                {
                    UserId = userId,
                    StartAddress = start,
                    EndAddress = new UnresolvedLocation(directionsQueryParameters.EndAddress),
                    DateTime = directionsQueryParameters.DepartureTime.HasValue ?
                        directionsQueryParameters.DepartureTime.Value :
                        directionsQueryParameters.ArrivalTime.Value,
                    ArriveBy = directionsQueryParameters.ArrivalTime.HasValue
                });
                Response.Headers.Add("ETag", $"\"{res.CacheKey}\"");
                return Ok(res.TransitDirections);
            }
            catch (LocationNotFoundException e)
            {
                _logger.LogError(e, "Could not resolve location");
                return NotFound();
            }
        }

        [HttpGet("{userId}/directions/transit")]
        [Authorize("Service")]
        public async Task<IActionResult> GetForUser(string userId, [FromQuery]DirectionsQueryParameters directionsQueryParameters)
        {
            return await GetDirections(userId, directionsQueryParameters);
        }

        [HttpGet("me/directions/transit")]
        [Authorize("User")]
        public async Task<IActionResult> GetForMe([FromQuery]DirectionsQueryParameters directionsQueryParameters)
        {
            return await GetDirections(User.GetId(), directionsQueryParameters);
        }

        [HttpGet("directions/{cacheKey}")]
        public async Task<IActionResult> Get(string cacheKey)
        {
            var res = await directionsCache.GetAsync(cacheKey);
            if (null != res)
            {
                Response.Headers.Add("ETag", $"\"{res.CacheKey}\"");
                return Ok(res.TransitDirections);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
