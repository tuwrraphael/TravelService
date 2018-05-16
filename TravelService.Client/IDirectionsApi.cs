namespace TravelService.Client
{
    public interface IDirectionsApi
    {
        ITransitApi Transit { get; }
    }
}
