using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl.OpenTripPlanner
{
    public class OpenTripPlannerClient : IOpenTripPlannerClient
    {
        private readonly HttpClient _client;

        public OpenTripPlannerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<OpenTripPlannerResponse> Plan(OpenTripPlannerRequest request)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fromPlace"] = $"{request.FromPlace.Lat.ToString(CultureInfo.InvariantCulture)},{request.FromPlace.Lng.ToString(CultureInfo.InvariantCulture)}";
            query["toPlace"] = $"{request.ToPlace.Lat.ToString(CultureInfo.InvariantCulture)},{request.ToPlace.Lng.ToString(CultureInfo.InvariantCulture)}";

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var viennaTime = request.DateTime.ToOffset(timeZoneInfo.GetUtcOffset(request.DateTime));
            query["date"] = viennaTime.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            query["time"] = viennaTime.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            query["arriveBy"] = request.ArriveBy ? "true" : "false";
            query["mode"] = "TRANSIT,WALK";
            query["maxWalkDistance"] = "1000";
            var res = await _client.GetAsync($"otp/routers/default/plan?{query.ToString()}");
            if (!res.IsSuccessStatusCode)
            {
                throw new OpenTripPlannerException($"Request to OpenTripPlanner returned : {res.StatusCode}");
            }
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OpenTripPlannerResponse>(content);
        }
    }

    public class OpenTripPlannerRequest
    {
        public Coordinate FromPlace { get; set; }
        public Coordinate ToPlace { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public bool ArriveBy { get; set; }
    }

    public class OpenTripPlannerResponse
    {
        public Plan Plan { get; set; }
    }

    public class Plan
    {
        public Itinerary[] Itineraries { get; set; }
        public Place From { get; set; }
        public Place To { get; set; }
    }

    public class Itinerary
    {
        public int Duration { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public Leg[] Legs { get; set; }
    }

    public class Place
    {
        public string Name { get; set; }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public int? StopIndex { get; set; }
    }

    public class Leg
    {
        public bool TransitLeg { get; set; }
        public Place From { get; set; }
        public Place To { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public int Duration { get; set; }
        public string Headsign { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public string Mode { get; set; }
    }

    public class OpenTripPlannerProvider : ITransitDirectionProvider
    {
        private readonly IOpenTripPlannerClient _openTripPlannerClient;

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
                    Line = new Models.Line()
                    {
                        Name = l.RouteLongName,
                        ShortName = l.RouteShortName,
                        VehicleType = l.Mode
                    },
                    NumStops = l.From.StopIndex.HasValue && l.To.StopIndex.HasValue ? (l.To.StopIndex.Value - l.From.StopIndex.Value) : 0
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
