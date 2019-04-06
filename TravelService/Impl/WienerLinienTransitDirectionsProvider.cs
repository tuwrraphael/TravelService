using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Services;

namespace TravelService.Impl
{
    public class WienerLinienRoutingClient : IWienerLinienRoutingClient
    {
        private readonly HttpClient _client;

        public WienerLinienRoutingClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<WLRoutingResponse> RequestTripAsync(DirectionsRequest directionsRequest)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["language"] = "de";
            query["outputFormat"] = "json";
            if (null != directionsRequest.StartAddress.Coordinate)
            {
                query["type_origin"] = "coord";
                query["name_origin"] = $"{directionsRequest.StartAddress.Coordinate.Lng}:{directionsRequest.StartAddress.Coordinate.Lat}:WGS84";
            }
            else
            {
                query["type_origin"] = "any";
                query["name_origin"] = directionsRequest.StartAddress.Address;
            }
            if (null != directionsRequest.EndAddress.Coordinate)
            {
                query["type_destination"] = "coord";
                query["name_destination"] = $"{directionsRequest.EndAddress.Coordinate.Lng}:{directionsRequest.EndAddress.Coordinate.Lat}:WGS84";
            }
            else
            {
                query["type_destination"] = "any";
                query["name_destination"] = directionsRequest.EndAddress.Address;
            }
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            if (directionsRequest.DepartureTime.HasValue)
            {
                var viennaTime = directionsRequest.DepartureTime.Value.ToOffset(timeZoneInfo.GetUtcOffset(directionsRequest.DepartureTime.Value));
                query["itdTripDateTimeDepArr"] = "dep";
                query["itdDate"] = viennaTime.DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                query["itdTime"] = viennaTime.DateTime.ToString("HHmm");
            }
            else if (directionsRequest.ArrivalTime.HasValue)
            {
                var viennaTime = directionsRequest.ArrivalTime.Value.ToOffset(timeZoneInfo.GetUtcOffset(directionsRequest.ArrivalTime.Value));
                query["itdTripDateTimeDepArr"] = "arr";
                query["itdDate"] = viennaTime.DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                query["itdTime"] = viennaTime.DateTime.ToString("HHmm");
            }
            var res = await _client.GetAsync($"ogd_routing/XML_TRIP_REQUEST2?{query.ToString()}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Request to WienerLinien returned : {res.StatusCode}");
            }
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WLRoutingResponse>(content);
        }
    }


    public class WLRoutingResponse
    {
        public Parameter[] parameters { get; set; }
        public Trip[] trips { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Trip
    {
        public Trip1 trip { get; set; }
    }

    public class Trip1
    {
        public string duration { get; set; }
        public string interchange { get; set; }
        public string desc { get; set; }
        public Leg[] legs { get; set; }
        public object[] attrs { get; set; }
    }

    public class Leg
    {
        public string timeMinute { get; set; }
        public Point[] points { get; set; }
        public Mode mode { get; set; }
        public Frequency frequency { get; set; }
        public string path { get; set; }
        public Turninst[] turnInst { get; set; }
        public Stopseq[] stopSeq { get; set; }
        public Interchange interchange { get; set; }
    }

    public class Mode
    {
        public string name { get; set; }
        public string number { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string destination { get; set; }
        public string destID { get; set; }
        public string desc { get; set; }
        public Diva diva { get; set; }
    }

    public class Diva
    {
        public string branch { get; set; }
        public string line { get; set; }
        public string supplement { get; set; }
        public string dir { get; set; }
        public string project { get; set; }
        public string network { get; set; }
        public string stateless { get; set; }
        public string _operator { get; set; }
        public string opCode { get; set; }
    }

    public class Frequency
    {
        public string hasFrequency { get; set; }
        public string tripIndex { get; set; }
        public string minTimeGap { get; set; }
        public string maxTimeGap { get; set; }
        public string avTimeGap { get; set; }
        public string minDuration { get; set; }
        public string maxDuration { get; set; }
        public string avDuration { get; set; }
        public object[] modes { get; set; }
    }

    public class Interchange
    {
        public string desc { get; set; }
        public string type { get; set; }
        public string path { get; set; }
    }

    public class Point
    {
        public string name { get; set; }
        public string place { get; set; }
        public string nameWithPlace { get; set; }
        public string usage { get; set; }
        public string omc { get; set; }
        public string placeID { get; set; }
        public string desc { get; set; }
        public WLDateTime dateTime { get; set; }
        public Stamp stamp { get; set; }
        public Link[] links { get; set; }
        public Ref _ref { get; set; }
    }

    public class WLDateTime
    {
        public string date { get; set; }
        public string time { get; set; }
    }

    public class Stamp
    {
        public string date { get; set; }
        public string time { get; set; }
    }

    public class Ref
    {
        public string id { get; set; }
        public string area { get; set; }
        public string platform { get; set; }
        public string NaPTANID { get; set; }
        public object[] attrs { get; set; }
        public string coords { get; set; }
    }

    public class Link
    {
        public string name { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    public class Turninst
    {
        public string dir { get; set; }
        public string manoeuvre { get; set; }
        public string name { get; set; }
        public string dirHint { get; set; }
        public string place { get; set; }
        public string tTime { get; set; }
        public string ctTime { get; set; }
        public string dis { get; set; }
        public string cDis { get; set; }
        public string coords { get; set; }
    }

    public class Stopseq
    {
        public string name { get; set; }
        public string nameWO { get; set; }
        public string place { get; set; }
        public string nameWithPlace { get; set; }
        public string omc { get; set; }
        public string placeID { get; set; }
        public string platformName { get; set; }
        public Ref1 _ref { get; set; }
    }

    public class Ref1
    {
        public string id { get; set; }
        public string area { get; set; }
        public string platform { get; set; }
        public string NaPTANID { get; set; }
        public object[] attrs { get; set; }
        public string coords { get; set; }
        public string depDateTime { get; set; }
        public string arrDelay { get; set; }
        public string depDelay { get; set; }
        public string arrDateTime { get; set; }
    }


    public class WienerLinienTransitDirectionsProvider : ITransitDirectionProvider
    {
        private readonly IWienerLinienRoutingClient _wienerLinienRoutingClient;

        public WienerLinienTransitDirectionsProvider(IWienerLinienRoutingClient wienerLinienRoutingClient)
        {
            _wienerLinienRoutingClient = wienerLinienRoutingClient;
        }

        private DateTimeOffset ParseWLDateTime(WLDateTime d)
        {
            var dt = DateTime.ParseExact($"{d.date}{d.time}", new string[] { "dd.MM.yyyy HH:mm" }, CultureInfo.InvariantCulture.DateTimeFormat);
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
                        Duration = TimeSpan.ParseExact(d.trip.duration, "HH:mm", CultureInfo.InvariantCulture.DateTimeFormat).TotalSeconds,
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
                                Duration = (legDepartureTime - legArrivalTime).TotalSeconds,
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
