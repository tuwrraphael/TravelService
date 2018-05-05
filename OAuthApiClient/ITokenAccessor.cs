using System;
using System.Threading.Tasks;

namespace OAuthApiClient
{
    public interface ITokenAccessor : IDisposable
    {
        StoredToken Get();
        Task Update(StoredToken tokens);
    }
}
