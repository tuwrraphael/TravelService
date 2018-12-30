using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Models.Locations;
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

        private string FormatDestination(ResolvedLocation resolvedLocation)
        {
            if (null != resolvedLocation.Coordinate)
            {
                return $"{resolvedLocation.Coordinate.Lat.ToString(CultureInfo.InvariantCulture)},{resolvedLocation.Coordinate.Lng.ToString(CultureInfo.InvariantCulture)}";
            }
            if (null != resolvedLocation.Attributes &&
                resolvedLocation.Attributes.ContainsKey("GoogleMapsPlaceId"))
            {
                return $"place_id:{resolvedLocation.Attributes["GoogleMapsPlaceId"]}";
            }
            return resolvedLocation.Address;
        }

        private string FormatStart(UserLocation startAddress)
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

        public async Task<TransitDirections> GetDirectionsAsync(UserLocation startAddress, ResolvedLocation endAddress, DateTimeOffset arrivalTime)
        {
            var request = new GoogleMapsApi.Entities.Directions.Request.DirectionsRequest()
            {
                Destination = FormatDestination(endAddress),
                Origin = FormatStart(startAddress),
                ArrivalTime = arrivalTime.UtcDateTime,
                TravelMode = GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit,
                ApiKey = options.GoogleMapsApiKey
            };
            var res = await GoogleMapsApi.GoogleMaps.Directions.QueryAsync(request);

            if (!res.Routes.Any())
            {
                return null;
            }
            var routes = res.Routes
                .Where(v => v.Legs.Any() && v.Legs.First().Steps.Any(b => b.TravelMode == GoogleMapsApi.Entities.Directions.Request.TravelMode.Transit))
                .Select(v => v.Legs.First()).Select(v => new Route()
                {
                    ArrivalTime = new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero).Add(v.ArrivalTime.Value),
                    DepatureTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(v.DepartureTime.Value),
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
                        ArrivalTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(step.TransitDetails.ArrivalTime.Value),
                        DepartureTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(step.TransitDetails.DepartureTime.Value),
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
