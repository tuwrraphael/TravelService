using TravelService.Client.ApiDefinition;

namespace TravelService.Client
{
    public interface ITravelServiceClient
    {
        IDirectionsApi Directions { get; }
        IUsers Users { get; }
    }
}
