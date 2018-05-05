using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface IDirectionService
    {
        Task<TransitDirections> GetTransitAsync(string startAddress, string endAddress, DateTime arrivalTime);
        Task<TransitDirections> GetTransitAsync(Coordinate start, string endAddress, DateTime arrivalTime);
        Task<TransitDirections> GetTransitForUserAsync(string userId, string endAddress, DateTime arrivalTime);
    }
}