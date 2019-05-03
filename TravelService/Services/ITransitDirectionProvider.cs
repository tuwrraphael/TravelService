using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Services
{

    public interface ITransitDirectionProvider
    {
        Task<Plan> GetDirectionsAsync(TransitDirectionsRequest request);
    }
}