using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client
{
    public interface ITransitApi
    {
        Task<DirectionsResult> Get(string endAddress, DateTimeOffset arrivalTime, string userId);
        Task<DirectionsResult> Get(string startAddress, string endAddress, DateTimeOffset arrivalTime);
        Task<DirectionsResult> Get(Coordinate startAddress, string endAddress, DateTimeOffset arrivalTime);
    }
}
