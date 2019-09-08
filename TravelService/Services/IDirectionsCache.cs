using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Services
{
    public interface IDirectionsCache
    {
        Task PutAsync(string key, Plan directions);
        Task<Plan> GetAsync(string key);
    }
}
