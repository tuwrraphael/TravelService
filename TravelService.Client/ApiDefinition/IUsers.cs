namespace TravelService.Client.ApiDefinition
{
    public interface IUsers
    {
        IUserApi Me { get; }
        IUserApi this[string userId] { get; }
    }
}
