using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Impl.OpenRouteService.Client;
using TravelService.Impl.WienerLinien.RoutingClient;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Services;

namespace TravelService.Impl.WienerLinien
{

    public class CombinedTransitDirectionsProvider : ITransitDirectionProvider
    {
        private readonly ITile38WLApiClient _tile38WLApiClient;
        private readonly IWienerLinienRoutingClient _wienerLinienRoutingClient;
        private readonly IOpenRouteServiceClient _openRouteServiceClient;

        public CombinedTransitDirectionsProvider(ITile38WLApiClient tile38WLApiClient,
            IWienerLinienRoutingClient wienerLinienRoutingClient, IOpenRouteServiceClient openRouteServiceClient)
        {
            _tile38WLApiClient = tile38WLApiClient;
            _wienerLinienRoutingClient = wienerLinienRoutingClient;
            _openRouteServiceClient = openRouteServiceClient;
        }

        private class RouteResponse
        {
            public Station From { get; set; }
            public TimeSpan FromTravelTime { get; set; }
            public Station To { get; set; }
            public TimeSpan ToTravelTime { get; set; }
            public WLRoutingResponse Response { get; set; }
        }

        private DateTimeOffset ParseWLDateTime(WLDateTime d)
        {
            var dt = DateTime.ParseExact($"{d.date} {d.time}", new string[] { "dd.MM.yyyy HH:mm" }, CultureInfo.InvariantCulture);
            var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            return new DateTimeOffset(dt, tz.GetUtcOffset(dt));
        }

        public async Task<TransitDirections> GetDirectionsAsync(DirectionsRequest request)
        {
            if (null == request.StartAddress.Coordinate || null == request.EndAddress.Coordinate)
            {
                throw new Exception("Combined route service only supports coordinate requests");
            }
            var fromStations = await _tile38WLApiClient.GetNearbyStations(request.StartAddress.Coordinate, 1500, 5);
            var toStations = await _tile38WLApiClient.GetNearbyStations(request.EndAddress.Coordinate, 1500, 5);
            var fromDistances = await _openRouteServiceClient.GetMatrix(request.StartAddress.Coordinate, fromStations.Select(v => new TravelTarget()
            {
                Id = v.Id,
                Target = new Coordinate()
                {
                    Lat = v.Lat,
                    Lng = v.Lng
                }
            }).ToArray());
            var toDistances = await _openRouteServiceClient.GetMatrix(request.EndAddress.Coordinate, toStations.Select(v => new TravelTarget()
            {
                Id = v.Id,
                Target = new Coordinate()
                {
                    Lat = v.Lat,
                    Lng = v.Lng
                }
            }).ToArray());
            List<RouteResponse> responses = new List<RouteResponse>();
            foreach (var from in fromStations)
            {
                foreach (var to in toStations)
                {
                    var req = new DirectionsRequest();
                    var toTravelTime = TimeSpan.FromSeconds(toDistances.Durations.Where(v => v.Key.Id == to.Id).Single().Value);
                    var fromTravelTime = TimeSpan.FromSeconds(fromDistances.Durations.Where(v => v.Key.Id == from.Id).Single().Value);
                    if (request.ArrivalTime.HasValue)
                    {
                        req.ArrivalTime = request.ArrivalTime.Value - toTravelTime;
                    }
                    else if (request.DepartureTime.HasValue)
                    {
                        req.DepartureTime = req.DepartureTime + fromTravelTime;
                    }
                    try
                    {
                        responses.Add(new RouteResponse
                        {
                            From = from,
                            To = to,
                            Response = await _wienerLinienRoutingClient.RequestTripAsync(from.Diva, to.Diva, new DirectionsRequest()
                            {

                            }),
                            FromTravelTime = fromTravelTime,
                            ToTravelTime = toTravelTime
                        });
                    }catch
                    {

                    }
                }
            }
            var routes = responses
              .Select(r => r.Response.trips.Select(d =>
              {
                  Point departurePoint = d.trip.legs.First().points.Where(v => v.usage == "departure").Single();
                  var departureTime = ParseWLDateTime(departurePoint.dateTime) - r.FromTravelTime;
                  Point arrivalPoint = d.trip.legs.Last().points.Where(v => v.usage == "arrival").Single();
                  var arrivalTime = ParseWLDateTime(arrivalPoint.dateTime) + r.ToTravelTime;
                  var duration = (TimeSpan.ParseExact(d.trip.duration, "hh':'mm", CultureInfo.InvariantCulture)
                   + r.FromTravelTime + r.ToTravelTime).TotalSeconds;
                  return new Route()
                  {
                      ArrivalTime = arrivalTime,
                      DepatureTime = departureTime,
                      Duration = duration,
                      StartAddress = r.From.Name,
                      EndAddress = r.To.Name,
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
              })).SelectMany(v => v).OrderBy(v => v.Duration);
            return new TransitDirections()
            {
                Routes = routes.ToArray()
            };
        }
    }
}

