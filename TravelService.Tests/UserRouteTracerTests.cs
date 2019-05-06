using System;
using System.Globalization;
using System.Linq;
using TravelService.Impl;
using TravelService.Models;
using Xunit;

namespace TravelService.Tests
{
    public class UserRouteTracerTests
    {
        [Fact]
        public void Test1()
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
                        }
                    }
                }
            };
            var result = userRouteTracer.TraceUserOnItinerary(i, new UserLocation()
            {
                Coordinate = new Coordinate(48.239071, 16.451171),
                Accuracy = new UserLocationAccuracy()
                {
                    Confidence = 0.68,
                    Radius = 20
                }
            });
            var denseLine = userRouteTracer.InsertPointsBetween(i.Legs.SelectMany(v => v.Geometry).ToList());
            var debugOutput = $"[{string.Join(",\n", denseLine.Select(v => $"[{v.Lat.ToString(CultureInfo.InvariantCulture)},{v.Lng.ToString(CultureInfo.InvariantCulture)}]"))}";
        }
    }
}
