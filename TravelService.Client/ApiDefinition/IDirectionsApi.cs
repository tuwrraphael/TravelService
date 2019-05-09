namespace TravelService.Client.ApiDefinition
{
    public interface IDirectionsApi
    {
        ITransitApi Transit { get; }
        IDirectionApi this[string cacheKey] { get; }
    }
}
