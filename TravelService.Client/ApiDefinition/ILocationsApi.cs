namespace TravelService.Client.ApiDefinition
{
    public interface ILocationsApi
    {
        ILocationApi this[string location] { get; }
    }
}
