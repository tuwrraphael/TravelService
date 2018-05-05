using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Services
{
    public interface ILocationProvider
    {
        Task<string> GetUserLocationAsync(string userId);
    }
}