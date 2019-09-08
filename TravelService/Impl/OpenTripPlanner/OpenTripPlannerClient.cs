using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TravelService.Models;

namespace TravelService.Impl.OpenTripPlanner
{
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
        public string Id { get; set; }
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
        public Geometry LegGeometry { get; set; }
    }

    public class Geometry
    {
        public string Points { get; set; }
    }
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
            var data = JsonConvert.DeserializeObject<OpenTripPlannerResponse>(content);
            if (null != res.Headers.ETag)
            {
                data.Id = res.Headers.ETag.Tag.Replace("\"", string.Empty);
            }
            return data;
        }

        public async Task Subscribe(string routeId, Uri callback)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["callback"] = callback.ToString();
            var res = await _client.PostAsync($"routes/{routeId}/subscribe?{query.ToString()}", null);
            if (!res.IsSuccessStatusCode)
            {
                throw new OpenTripPlannerException($"Subscribe returned : {res.StatusCode}");
            }
        }
    }
}
