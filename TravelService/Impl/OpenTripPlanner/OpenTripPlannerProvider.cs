using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl.OpenTripPlanner
{
    public class OpenTripPlannerProvider : ITransitDirectionProvider
    {
        private readonly IOpenTripPlannerClient _openTripPlannerClient;

        private static Coordinate[] DecodePolyLine(string encodedPolyLine)
        {
            if (string.IsNullOrEmpty(encodedPolyLine))
            {
                return null;
            }
            char[] polylineChars = encodedPolyLine.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;
            var coords = new List<Coordinate>();
            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                coords.Add(new Coordinate(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));
            }
            return coords.ToArray();
        }


        public OpenTripPlannerProvider(IOpenTripPlannerClient openTripPlannerClient)
        {
            _openTripPlannerClient = openTripPlannerClient;
        }

        public async Task<Models.Plan> GetDirectionsAsync(TransitDirectionsRequest request)
        {
            var res = await _openTripPlannerClient.Plan(new OpenTripPlannerRequest()
            {
                FromPlace = request.From,
                ToPlace = request.To,
                ArriveBy = request.ArriveBy,
                DateTime = request.DateTime
            });
            if (null == res.Plan)
            {
                return null;
            }
            var routes = res.Plan.Itineraries.Select(i => new Models.Itinerary()
            {
                EndTime = DateTimeOffset.FromUnixTimeMilliseconds(i.EndTime),
                StartTime = DateTimeOffset.FromUnixTimeMilliseconds(i.StartTime),

                Legs = i.Legs.Where(l => l.TransitLeg).Select(l => new Models.Leg()
                {
                    To = new Models.Place()
                    {
                        Coordinate = new Coordinate()
                        {
                            Lat = l.To.Lat,
                            Lng = l.To.Lon
                        },
                        Name = l.To.Name
                    },
                    From = new Models.Place()
                    {
                        Coordinate = new Coordinate()
                        {
                            Lat = l.From.Lat,
                            Lng = l.From.Lon
                        },
                        Name = l.From.Name
                    },
                    EndTime = DateTimeOffset.FromUnixTimeMilliseconds(l.EndTime),
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(l.StartTime),
                    Headsign = l.Headsign,
                    Line = new Line()
                    {
                        Name = l.RouteLongName,
                        ShortName = l.RouteShortName,
                        VehicleType = l.Mode
                    },
                    NumStops = l.From.StopIndex.HasValue && l.To.StopIndex.HasValue ? (l.To.StopIndex.Value - l.From.StopIndex.Value) : 0,
                    Geometry = DecodePolyLine(l.LegGeometry?.Points)
                }).ToArray()
            }).ToArray();
            return new Models.Plan()
            {
                Itineraries = routes,
                From = new Models.Place()
                {
                    Coordinate = new Coordinate()
                    {
                        Lat = res.Plan.From.Lat,
                        Lng = res.Plan.From.Lon
                    },
                    Name = res.Plan.From.Name
                },
                To = new Models.Place()
                {
                    Coordinate = new Coordinate()
                    {
                        Lat = res.Plan.To.Lat,
                        Lng = res.Plan.To.Lon
                    },
                    Name = res.Plan.To.Name
                }
            };
        }
    }
}
