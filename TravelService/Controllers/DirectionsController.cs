using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Controllers
{
    [Route("api/")]
    public class DirectionsController : Controller
    {
        private readonly IDirectionService directionsService;
        private readonly IDirectionsCache directionsCache;
        private readonly ILocationsService locationsService;

        public DirectionsController(IDirectionService directionsService,
            IDirectionsCache directionsCache, ILocationsService locationsService)
        {
            this.directionsService = directionsService;
            this.directionsCache = directionsCache;
            this.locationsService = locationsService;
        }

        [HttpGet("directions/transit")]
        public async Task<IActionResult> Get([FromQuery]DirectionsQueryParameters directionsQueryParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (directionsQueryParameters.ArrivalTime.HasValue == directionsQueryParameters.DepartureTime.HasValue)
            {
                return BadRequest("Either departureTime xor arrialTime must be specified.");
            }
            UserLocation start;
            if (null != directionsQueryParameters.StartAddress)
            {
                start = new UserLocation(directionsQueryParameters.StartAddress);

            }
            else if (directionsQueryParameters.StartLat.HasValue && directionsQueryParameters.StartLng.HasValue)
            {
                start = new UserLocation(new Coordinate()
                {
                    Lat = directionsQueryParameters.StartLat.Value,
                    Lng = directionsQueryParameters.StartLng.Value
                });
            }
            else
            {
                return BadRequest();
            }
            var res = await directionsService.GetTransitAsync(new DirectionsRequest()
            {
                StartAddress = start,
                EndAddress = await locationsService.ResolveAnonymousAsync(directionsQueryParameters.EndAddress, start),
                DepartureTime = directionsQueryParameters.DepartureTime,
                ArrivalTime = directionsQueryParameters.ArrivalTime
            });
            Response.Headers.Add("ETag", $"\"{res.CacheKey}\"");
            return Ok(res.TransitDirections);
        }

        private async Task<IActionResult> GetForUser(string endAddress, DateTimeOffset arrivalTime, string userId)
        {
            return NotFound();
        }

        [HttpGet("{userId}/directions/transit")]
        [Authorize("Service")]
        public async Task<IActionResult> Get(string endAddress, DateTimeOffset arrivalTime, string userId)
        {
            return await GetForUser(endAddress, arrivalTime, userId);
        }

        [HttpGet("me/directions/transit")]
        [Authorize("User")]
        public async Task<IActionResult> Get(string endAddress, DateTimeOffset arrivalTime)
        {
            return await GetForUser(endAddress, arrivalTime, User.GetId());
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
