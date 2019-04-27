using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl.OpenRouteService.Client
{
    public interface IOpenRouteServiceClient
    {
        Task<OpenRouteGeocodeResult> Geocode(string term, Coordinate coord);
        Task<TravelTimes> GetMatrix(Coordinate from, TravelTarget[] to);
    }
}