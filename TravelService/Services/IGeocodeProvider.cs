using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Services
{
    public interface IGeocodeProvider
    {
        Task<string> GetAddressAsync(Coordinate start);
    }
}