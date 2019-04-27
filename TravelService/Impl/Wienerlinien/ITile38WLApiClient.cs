using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Impl.WienerLinien
{
    public interface ITile38WLApiClient
    {
        Task<Station[]> GetNearbyStations(Coordinate coordinate, double distance, int limit);
    }
}