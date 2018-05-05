using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Controllers
{
    public interface ILocationProviderConfigurationService
    {
        Task<LocationProviderConfiguration[]> GetConfigurations(string userId);
        Task<LocationProviderConfiguration> AddProvider(string userId, LocationProviderConfiguration config);
        Task AddStatic(string userId);
    }
}