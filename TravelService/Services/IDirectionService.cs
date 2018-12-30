using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Services
{
    public interface IDirectionService
    {
        Task<DirectionsResult> GetTransitAsync(string startAddress, string endAddress, DateTimeOffset arrivalTime);
        Task<DirectionsResult> GetTransitAsync(Coordinate start, string endAddress, DateTimeOffset arrivalTime);
        Task<DirectionsResult> GetTransitForUserAsync(string userId, string endAddress, DateTimeOffset arrivalTime);
    }
}