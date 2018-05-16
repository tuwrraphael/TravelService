using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client
{
    public interface ITransitApi
    {
        Task<TransitDirections> Get(string endAddress, DateTime arrivalTime, string userId);
        Task<TransitDirections> Get(string startAddress, string endAddress, DateTime arrivalTime);
        Task<TransitDirections> Get(Coordinate startAddress, string endAddress, DateTime arrivalTime);
    }
}
