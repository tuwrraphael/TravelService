namespace TravelService.Client
{
    public interface IUserApi
    {
        IUserDirectionApi Directions { get; }
    }
}
