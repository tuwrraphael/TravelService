using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravelService.Impl;
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
        private readonly IUserRouteTracer _userRouteTracer;
        private readonly ISubscriptionService _subscriptionService;

        public DirectionsController(IDirectionService directionsService,
            IDirectionsCache directionsCache, ILogger<DirectionsController> logger,
            IUserRouteTracer userRouteTracer,
            ISubscriptionService subscriptionService)
        {
            this.directionsService = directionsService;
            this.directionsCache = directionsCache;
            _logger = logger;
            _userRouteTracer = userRouteTracer;
            _subscriptionService = subscriptionService;
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
            var endAdress = new UnresolvedLocation(directionsQueryParameters.EndAddress);
            try
            {
                var res = await directionsService.GetTransitAsync(new DirectionsRequest()
                {
                    UserId = userId,
                    StartAddress = start,
                    EndAddress = endAdress,
                    DateTime = directionsQueryParameters.DepartureTime.HasValue ?
                        directionsQueryParameters.DepartureTime.Value :
                        directionsQueryParameters.ArrivalTime.Value,
                    ArriveBy = directionsQueryParameters.ArrivalTime.HasValue
                });
                if (null == res)
                {
                    return NotFound(new DirectionsNotFoundResult()
                    {
                        Reason = DirectionsNotFoundReason.RouteNotFound,
                        EndAddressFound = true,
                        StartAddressFound = true
                    });
                }
                Response.Headers.Add("ETag", $"\"{res.CacheKey}\"");

                return Ok(res.TransitDirections);
            }
            catch (LocationNotFoundException e)
            {
                return NotFound(new DirectionsNotFoundResult()
                {
                    Reason = DirectionsNotFoundReason.AddressNotFound,
                    EndAddressFound = e.UnresolvedLocation == endAdress,
                    StartAddressFound = e.UnresolvedLocation != endAdress
                });
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
                Response.Headers.Add("ETag", $"\"{cacheKey}\"");
                return Ok(res.GetTransitDirections());
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("directions/{cacheKey}/subscribe")]
        public async Task<IActionResult> Subscribe(string cacheKey, string callback)
        {
            var res = await directionsCache.GetAsync(cacheKey);
            if (null != res)
            {
                var id = await _subscriptionService.Subscribe(cacheKey, callback);
                return StatusCode(201, id);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("directions/{cacheKey}/itineraries/{index}/trace")]
        public async Task<IActionResult> Get(string cacheKey, int index, [FromQuery]TraceQueryParameters location)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var res = await directionsCache.GetAsync(cacheKey);
            if (null != res && index < res.Itineraries.Length)
            {
                return Ok(_userRouteTracer.TraceUserWithParticles(res.Itineraries[index], new TraceLocation
                {
                    Accuracy = new TraceLocationAccuracy()
                    {
                        Confidence = location.AccuracyConfidence,
                        Radius = location.AccuracyRadius
                    },
                    Coordinate = new Coordinate(location.Lat, location.Lng),
                    Timestamp = location.Timestamp
                }));
            }
            else
            {
                return NotFound();
            }
        }
    }
}
