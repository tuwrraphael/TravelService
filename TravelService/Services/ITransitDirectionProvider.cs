using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
using TravelService.Models.Locations;

namespace TravelService.Services
{
    public interface ITransitDirectionProvider
    {
        Task<TransitDirections> GetDirectionsAsync(UserLocation startAddress, ResolvedLocation endAddress, DateTime arrivalTime);
    }
}