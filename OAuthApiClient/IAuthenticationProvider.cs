using System.Net.Http;
using System.Threading.Tasks;

namespace OAuthApiClient
{
    public interface IAuthenticationProvider
    {
        Task AuthenticateClient(HttpClient client);
    }
}
