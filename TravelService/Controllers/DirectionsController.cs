using System;
using System.ComponentModel.DataAnnotations;
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

        public DirectionsController(IDirectionService directionsService)
        {
            this.directionsService = directionsService;
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
            if (directionsQueryParameters.StartLat.HasValue  && directionsQueryParameters.StartLng.HasValue)
            {
                return Ok(await directionsService.GetTransitAsync(new Coordinate() {
                    Lat = directionsQueryParameters.StartLat.Value,
                    Lng = directionsQueryParameters.StartLng.Value
                }, directionsQueryParameters.EndAddress, directionsQueryParameters.ArrivalTime));
            }
            return BadRequest();
        }

        [HttpGet("{userId}/directions/transit")]
        [Authorize("Service")]
        public async Task<IActionResult> Get(string endAddress, DateTime arrivalTime, string userId)
        {
            var directions = await directionsService.GetTransitForUserAsync(userId, endAddress, arrivalTime);
            return Ok(directions);
        }

        [HttpGet("me/directions/transit")]
        [Authorize("User")]
        public async Task<IActionResult> Get(string endAddress, DateTime arrivalTime)
        {
            var directions = await directionsService.GetTransitForUserAsync(User.GetId(), endAddress, arrivalTime);
            return Ok(directions);
        }
    }
}
