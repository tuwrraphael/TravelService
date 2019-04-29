namespace TravelService.Client
{
    public interface IUsers
    {
        IUserApi Me { get; }
        IUserApi this[string userId] { get; }
    }
}
