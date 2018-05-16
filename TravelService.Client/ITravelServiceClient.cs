using System.Threading.Tasks;

namespace TravelService.Client
{
    public interface ITravelServiceClient
    {
        IDirectionsApi Directions { get; }
    }
}
