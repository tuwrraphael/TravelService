namespace TravelService.Client
{
    public interface IDirectionsApi
    {
        ITransitApi Transit { get; }
        IDirectionApi this[string cacheKey] { get; }
    }
}
