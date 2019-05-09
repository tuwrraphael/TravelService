namespace TravelService.Client.ApiDefinition
{
    public interface IUserApi
    {
        IUserDirectionApi Directions { get; }
        ILocationsApi Locations { get; }
    }
}
