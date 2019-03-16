using System.Threading.Tasks;
using TravelService.Models.Directions;

namespace TravelService.Client
{
    public interface IDirectionApi
    {
        Task<DirectionsResult> GetAsync();
    }
}