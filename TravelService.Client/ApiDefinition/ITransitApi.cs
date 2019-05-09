using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;

namespace TravelService.Client.ApiDefinition
{
    public interface ITransitApi
    {
        Task<DirectionsResult> Get(string startAddress, string endAddress, DateTimeOffset? arrivalTime = null, DateTimeOffset? departueTime = null);
        Task<DirectionsResult> Get(Coordinate startAddress, string endAddress, DateTimeOffset? arrivalTime = null, DateTimeOffset? departueTime = null);
    }
}
