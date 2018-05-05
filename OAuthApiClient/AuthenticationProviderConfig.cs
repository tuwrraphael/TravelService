using System;

namespace OAuthApiClient
{
    public class AuthenticationProviderConfig
    {
        public Uri ServiceIdentityBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; }
        public string Name { get; set; }
    }
}
