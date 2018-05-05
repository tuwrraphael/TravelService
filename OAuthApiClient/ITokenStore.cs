using System.Threading.Tasks;

namespace OAuthApiClient
{
    public interface ITokenStore
    {
        Task<ITokenAccessor> Get(string name);
    }
}
