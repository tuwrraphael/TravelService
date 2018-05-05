using System;
using System.Threading.Tasks;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface ITransitDirectionProvider
    {
        Task<TransitDirections> GetDirectionsAsync(string startAddress, string endAddress, DateTime arrivalTime);
    }
}