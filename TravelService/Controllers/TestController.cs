using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelService.Impl;
using TravelService.Models;

namespace TravelService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult TestTraceRoute(double lat, double lng, double acc)
        {
            var userRouteTracer = new UserRouteTracer();
            var i = new Itinerary()
            {
                Legs = new[] {
                    new Leg()
                    {
                        Geometry = new []{
                            new Coordinate(48.237986, 16.453807),
                        new Coordinate(48.238296, 16.453866),
                        new Coordinate(48.238812, 16.452647),
                        new Coordinate(48.238976, 16.451767),
                        new Coordinate(48.239520, 16.448791)
                        },
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddMinutes(10)
                    },
                }
            };
            var result = userRouteTracer.TraceUserWithParticles(i, new TraceLocation()
            {
                Coordinate = new Coordinate(lat, lng),
                Accuracy = new TraceLocationAccuracy()
                {
                    Confidence = 0.68,
                    Radius = acc
                },
                Timestamp = DateTime.Now.AddMinutes(4)
            });
            return Ok(result);
        }
    }
}