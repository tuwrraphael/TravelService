using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Services;

namespace TravelService.Impl
{
    public class WienerLinienTransitDirectionsProvider : ITransitDirectionProvider
    {
        private readonly IWienerLinienRoutingClient _wienerLinienRoutingClient;

        public WienerLinienTransitDirectionsProvider(IWienerLinienRoutingClient wienerLinienRoutingClient)
        {
            _wienerLinienRoutingClient = wienerLinienRoutingClient;
        }

        private DateTimeOffset ParseWLDateTime(WLDateTime d)
        {
            var dt = DateTime.ParseExact($"{d.date} {d.time}", new string[] { "dd.MM.yyyy HH:mm" }, CultureInfo.InvariantCulture);
            var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            return new DateTimeOffset(dt, tz.GetUtcOffset(dt));
        }

        public async Task<TransitDirections> GetDirectionsAsync(DirectionsRequest request)
        {
            var directions = await _wienerLinienRoutingClient.RequestTripAsync(request);
            if (null == directions || directions.trips == null || !directions.trips.Any())
            {
                return null;
            }

            var routes = directions.trips
                .Select(d =>
                {
                    Point departurePoint = d.trip.legs.First().points.Where(v => v.usage == "departure").Single();
                    var departureTime = ParseWLDateTime(departurePoint.dateTime);
                    Point arrivalPoint = d.trip.legs.Last().points.Where(v => v.usage == "arrival").Single();
                    var arrivalTime = ParseWLDateTime(arrivalPoint.dateTime);
                    return new Route()
                    {
                        ArrivalTime = arrivalTime,
                        DepatureTime = departureTime,
                        Duration = TimeSpan.ParseExact(d.trip.duration, "hh':'mm", CultureInfo.InvariantCulture).TotalSeconds,
                        StartAddress = departurePoint.name,
                        EndAddress = arrivalPoint.name,
                        Steps = d.trip.legs.Where(f => f.mode.type != "100").Select(v =>
                        {
                            Point legDeparturePoint = v.points.Where(e => e.usage == "departure").Single();
                            var legDepartureTime = ParseWLDateTime(legDeparturePoint.dateTime);
                            Point legArrivalPoint = v.points.Where(e => e.usage == "arrival").Single();
                            var legArrivalTime = ParseWLDateTime(legArrivalPoint.dateTime);
                            return new Step()
                            {
                                ArrivalStop = new Stop()
                                {
                                    Name = legArrivalPoint.name
                                },
                                DepartureStop = new Stop()
                                {
                                    Name = legDeparturePoint.name
                                },
                                ArrivalTime = legArrivalTime,
                                DepartureTime = legDepartureTime,
                                Duration = (legArrivalTime - legDepartureTime).TotalSeconds,
                                Headsign = v.mode.destination,
                                Line = new Line()
                                {
                                    Name = v.mode.name,
                                    ShortName = v.mode.number,
                                    VehicleType = v.mode.type
                                },
                                NumStops = v.stopSeq.Count()
                            };
                        }).ToArray(),
                    };
                }).ToArray();
            return new TransitDirections()
            {
                Routes = routes
            };
        }
    }
}
