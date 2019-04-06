using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl.OpenRouteService.Client
{
    public interface IOpenRouteServiceClient
    {
        Task<OpenRouteGeocodeResult> Geocode(string term, Coordinate coord);
    }
}