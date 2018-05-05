using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Services;

namespace TravelService.Impl
{
    public class GoogleMapsTransitDirectionProvider : ITransitDirectionProvider
    {
        private readonly GoogleMapsApiOptions options;

        public GoogleMapsTransitDirectionProvider(IOptions<GoogleMapsApiOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
        }

        public async Task<TransitDirections> GetDirectionsAsync(string startAddress, string endAddress, DateTime arrivalTime)
        {
            var res = await GoogleMapsApi.GoogleMaps.Directions.QueryAsync(new GoogleMapsApi.Entities.Directions.Request.DirectionsRequest()
            {
                Origin = startAddress,
                Destination = endAddress,
                ArrivalTime = arrivalTime,
                TravelMode = GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit,
                ApiKey = options.GoogleMapsApiKey
            });

            if (!res.Routes.Any())
            {
                return null;
            }
            var routes = res.Routes
                .Where(v => v.Legs.Any() && v.Legs.First().Steps.Any(b => b.TravelMode == GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit))
                .Select(v => v.Legs.First()).Select(v => new Route()
                {
                    ArrivalTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(v.ArrivalTime.Value),
                    DepatureTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(v.DepartureTime.Value),
                    Duration = v.Duration.Value.TotalSeconds,
                    EndAddress = v.EndAddress,
                    EndLocation = v.EndLocation != null ? new Coordinate() { Lat = v.EndLocation.Latitude, Lng = v.EndLocation.Longitude } : null,
                    StartLocation = v.StartLocation != null ? new Coordinate() { Lat = v.StartLocation.Latitude, Lng = v.StartLocation.Longitude } : null,
                    StartAddress = v.StartAddress,
                    Steps = v.Steps.Where(step => step.TravelMode == GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit).Select(step => new Step()
                    {
                        ArrivalStop = null != step.TransitDetails?.ArrivalStop ? new Stop()
                        {
                            Location =
                                new Coordinate()
                                {
                                    Lat = step.TransitDetails.ArrivalStop.Location.Latitude,
                                    Lng = step.TransitDetails.ArrivalStop.Location.Longitude
                                },
                            Name = step.TransitDetails.ArrivalStop.Name
                        } : null,
                        DepartureStop = null != step.TransitDetails?.DepartureStop ? new Stop()
                        {
                            Location =
                                new Coordinate()
                                {
                                    Lat = step.TransitDetails.DepartureStop.Location.Latitude,
                                    Lng = step.TransitDetails.DepartureStop.Location.Longitude
                                },
                            Name = step.TransitDetails.DepartureStop.Name
                        } : null,
                        Duration = step.Duration.Value.TotalSeconds,
                        Headsign = step.TransitDetails.Headsign,
                        Line = null != step.TransitDetails.Lines ? new Line()
                        {
                            Name = step.TransitDetails.Lines.Name,
                            ShortName = step.TransitDetails.Lines.ShortName,
                            VehicleType = "Unknown"
                        } : null,
                        ArrivalTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(step.TransitDetails.ArrivalTime.Value),
                        DepartureTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(step.TransitDetails.DepartureTime.Value),
                        NumStops = step.TransitDetails.NumberOfStops
                    }).ToArray()
                }).ToArray();
            return new TransitDirections()
            {
                Routes = routes
            };
        }
    }
}
