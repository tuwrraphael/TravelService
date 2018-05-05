using System;

namespace OAuthApiClient
{
    public class StoredToken
    {
        public string AccessToken { get; set; }
        public DateTime? AccesTokenExpires { get; set; }

        public bool HasValidAccessToken => null != AccessToken && AccesTokenExpires.HasValue && AccesTokenExpires > DateTime.Now;
    }
}
