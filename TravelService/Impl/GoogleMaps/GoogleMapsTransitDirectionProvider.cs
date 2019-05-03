using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl.GoogleMaps
{
    public class GoogleMapsTransitDirectionProvider : ITransitDirectionProvider
    {
        private readonly ApiOptions options;

        public GoogleMapsTransitDirectionProvider(IOptions<ApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }

        private string FormatCoordinate(Coordinate coordinate)
        {
            return $"{coordinate.Lat.ToString(CultureInfo.InvariantCulture)},{coordinate.Lng.ToString(CultureInfo.InvariantCulture)}";

        }

        private string FormatStart(UnresolvedLocation startAddress)
        {
            if (null != startAddress.Coordinate)
            {
                return $"{startAddress.Coordinate.Lat.ToString(CultureInfo.InvariantCulture)},{startAddress.Coordinate.Lng.ToString(CultureInfo.InvariantCulture)}";
            }
            else
            {
                return startAddress.Address;
            }
        }

        public async Task<Plan> GetDirectionsAsync(TransitDirectionsRequest r)
        {
            var request = new GoogleMapsApi.Entities.Directions.Request.DirectionsRequest()
            {
                Destination = FormatCoordinate(r.To),
                Origin = FormatCoordinate(r.From),
                TravelMode = GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit,
                ApiKey = options.GoogleMapsApiKey
            };
            if (r.ArriveBy)
            {
                request.ArrivalTime = r.DateTime.UtcDateTime;
            }
            else
            {
                request.DepartureTime = r.DateTime.UtcDateTime;
            }
            var res = await GoogleMapsApi.GoogleMaps.Directions.QueryAsync(request);
            if (!res.Routes.Any())
            {
                return null;
            }
            var routes = res.Routes
                .Where(v => v.Legs.Any() && v.Legs.First().Steps.Any(b => b.TravelMode == GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit))
                .Select(v => v.Legs.First()).Select(v => new Itinerary()
                {
                    EndTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(v.ArrivalTime.Value),
                    StartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(v.DepartureTime.Value),
                    Legs = v.Steps.Where(step => step.TravelMode == GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit).Select(step => new Leg()
                    {
                        To = null != step.TransitDetails?.ArrivalStop ? new Place()
                        {
                            Coordinate =
                                new Coordinate()
                                {
                                    Lat = step.TransitDetails.ArrivalStop.Location.Latitude,
                                    Lng = step.TransitDetails.ArrivalStop.Location.Longitude
                                },
                            Name = step.TransitDetails.ArrivalStop.Name
                        } : null,
                        From = null != step.TransitDetails?.DepartureStop ? new Place()
                        {
                            Coordinate =
                                new Coordinate()
                                {
                                    Lat = step.TransitDetails.DepartureStop.Location.Latitude,
                                    Lng = step.TransitDetails.DepartureStop.Location.Longitude
                                },
                            Name = step.TransitDetails.DepartureStop.Name
                        } : null,
                        Headsign = step.TransitDetails.Headsign,
                        Line = null != step.TransitDetails.Lines ? new Models.Line()
                        {
                            Name = step.TransitDetails.Lines.Name,
                            ShortName = step.TransitDetails.Lines.ShortName,
                            VehicleType = "Unknown"
                        } : null,
                        EndTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(step.TransitDetails.ArrivalTime.Value),
                        StartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(step.TransitDetails.DepartureTime.Value),
                        NumStops = step.TransitDetails.NumberOfStops
                    }).ToArray()
                }).ToArray();
            return new Plan()
            {
                Itineraries = routes
            };
        }
    }
}
