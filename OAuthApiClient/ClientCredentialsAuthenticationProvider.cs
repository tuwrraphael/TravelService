using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace OAuthApiClient
{
    public class ClientCredentialsAuthenticationProvider : IAuthenticationProvider
    {
        private readonly ITokenStore tokenStore;
        private readonly AuthenticationProviderConfig config;

        public ClientCredentialsAuthenticationProvider(
            ITokenStore tokenStore, AuthenticationProviderConfig config)
        {
            this.tokenStore = tokenStore;
            this.config = config;
        }

        public async Task AuthenticateClient(HttpClient client)
        {
            using (var tokenAccessor = await tokenStore.Get(config.Name))
            {
                var tokens = tokenAccessor.Get();
                if (tokens.HasValidAccessToken)
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
                }
                else
                {
                    var tokenClient = new HttpClient();
                    var dict = new Dictionary<string, string>() {
                        { "client_id", config.ClientId},
                        { "scope", config.Scopes},
                        {"grant_type", "client_credentials" },
                        {"client_secret", config.ClientSecret }
                    };
                    var result = await client.PostAsync(new Uri(config.ServiceIdentityBaseUrl, "connect/token"),
                        new FormUrlEncodedContent(dict));
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new AuthenticationException($"Could not aquire token via client credentials. {result.StatusCode}");
                    }
                    var tokenResponse = await result.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse);
                    await tokenAccessor.Update(new StoredToken()
                    {
                        AccessToken = res.access_token,
                        AccesTokenExpires = DateTime.Now.AddSeconds(res.expires_in)
                    });
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", res.access_token);
                }
            }

        }
    }
}
