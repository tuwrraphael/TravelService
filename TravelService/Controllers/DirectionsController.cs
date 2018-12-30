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

        public DirectionsController(IDirectionService directionsService,
            IDirectionsCache directionsCache)
        {
            this.directionsService = directionsService;
            this.directionsCache = directionsCache;
        }

        [HttpGet("directions/transit")]
        public async Task<IActionResult> Get([FromQuery]DirectionsQueryParameters directionsQueryParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (null != directionsQueryParameters.StartAddress)
            {
                return Ok(await directionsService.GetTransitAsync(directionsQueryParameters.StartAddress, directionsQueryParameters.EndAddress, directionsQueryParameters.ArrivalTime));
            }
            if (directionsQueryParameters.StartLat.HasValue && directionsQueryParameters.StartLng.HasValue)
            {
                var res = await directionsService.GetTransitAsync(new Coordinate()
                {
                    Lat = directionsQueryParameters.StartLat.Value,
                    Lng = directionsQueryParameters.StartLng.Value
                }, directionsQueryParameters.EndAddress, directionsQueryParameters.ArrivalTime);
                Response.Headers.Add("ETag", $"\"{res.CacheKey}\"");
                return Ok(res.TransitDirections);
            }
            return BadRequest();
        }

        private async Task<IActionResult> GetForUser(string endAddress, DateTimeOffset arrivalTime, string userId)
        {
            try
            {
                var directions = await directionsService.GetTransitForUserAsync(userId, endAddress, arrivalTime);
                Response.Headers.Add("ETag", $"\"{directions.CacheKey}\"");
                return Ok(directions.TransitDirections);
            }
            catch (UserLocationNotFoundException)
            {
                return NotFound();
            }
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
