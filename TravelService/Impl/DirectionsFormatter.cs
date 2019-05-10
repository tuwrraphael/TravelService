using System.Linq;
using TravelService.Models.Directions;

namespace TravelService.Impl
{
    public static class PlanFormatter
    {
        public static TransitDirections GetTransitDirections(this Models.Plan plan)
        {
            return new TransitDirections()
            {
                Routes = plan.Itineraries.Select(i => Format(i, plan)).ToArray()
            };
        }

        public static Route Format(Models.Itinerary itinerary, Models.Plan plan)
        {
            return new Route()
            {
                ArrivalTime = itinerary.EndTime,
                DepatureTime = itinerary.StartTime,
                Duration = (itinerary.EndTime - itinerary.StartTime).TotalSeconds,
                EndLocation = plan.To.Coordinate,
                StartLocation = plan.From.Coordinate,
                Steps = itinerary.Legs.Where(v => v.TransitLeg).Select(Format).ToArray(),
            };
        }

        public static Step Format(Models.Leg leg)
        {
            return new Step()
            {
                ArrivalStop = Format(leg.To),
                ArrivalTime = leg.EndTime,
                DepartureStop = Format(leg.From),
                DepartureTime = leg.StartTime,
                Duration = (leg.EndTime - leg.StartTime).TotalSeconds,
                Headsign = leg.Headsign,
                Line = Format(leg.Line),
                NumStops = leg.NumStops
            };
        }

        public static Line Format(Models.Line line)
        {
            return new Line()
            {
                Name = line.Name,
                ShortName = line.ShortName,
                VehicleType = line.VehicleType
            };
        }

        public static Stop Format(Models.Place place)
        {
            return new Stop()
            {
                Location = place.Coordinate,
                Name = place.Name
            };
        }
    }
}
