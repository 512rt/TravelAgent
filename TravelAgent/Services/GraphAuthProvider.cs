using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using TravelAgent.Services.Interfaces;

namespace TravelAgent.Services
{
    public class GraphAuthProvider : IGraphAuthProvider
    {
        private readonly IConfiguration _config;
        private readonly ClientSecretCredential _credential;

        private AccessToken _cachedGraphAPIToken;
        private DateTimeOffset _cachedGraphAPIExpiry;

        private AccessToken _cachedSharePointAPIToken;
        private DateTimeOffset _cachedSharePointAPIExpiry;

        public GraphAuthProvider(IConfiguration config)
        {
            _config = config;

            _credential = new ClientSecretCredential(
                _config["AzureAd:TenantId"],
                _config["AzureAd:ClientId"],
                _config["AzureAd:ClientSecret"]);
        }

        public async Task<string> GetGraphAPIAccessTokenAsync()
        {
            try
            {
                // Only fetch new token if current one is null or near expiry
                if (!string.IsNullOrEmpty(_cachedGraphAPIToken.Token) &&
                    _cachedGraphAPIExpiry > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    return _cachedGraphAPIToken.Token;
                }

                // Get a new token and cache it
                var tokenContext = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
                _cachedGraphAPIToken = await _credential.GetTokenAsync(tokenContext);
                _cachedGraphAPIExpiry = _cachedGraphAPIToken.ExpiresOn;

                return _cachedGraphAPIToken.Token;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> GetSharePointAPIAccessTokenAsync()
        {
            try
            {
                // Only fetch new token if current one is null or near expiry
                if (!string.IsNullOrEmpty(_cachedSharePointAPIToken.Token) &&
                    _cachedSharePointAPIExpiry > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    return _cachedSharePointAPIToken.Token;
                }

                // Get a new token and cache it
                var tokenContext = new TokenRequestContext(new[] { "https://rastech905.sharepoint.com/.default" });
                _cachedSharePointAPIToken = await _credential.GetTokenAsync(tokenContext);
                _cachedSharePointAPIExpiry = _cachedSharePointAPIToken.ExpiresOn;

                return _cachedSharePointAPIToken.Token;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public TokenCredential GetTokenCredential() => _credential;
    }
}



//using Microsoft.Identity.Client;

//namespace TravelAgent.Services
//{
//    public class GraphAuthProvider : IGraphAuthProvider
//    {
//        private readonly IConfidentialClientApplication _app;
//        private readonly IConfiguration _config;
//        private AuthenticationResult _authResult;
//        private readonly string[] _scopes = new[] { "https://graph.microsoft.com/.default" };

//        public GraphAuthProvider(IConfiguration config)
//        {
//            _config = config;
//            _app = ConfidentialClientApplicationBuilder.Create(_config["AzureAd:ClientId"])
//                .WithClientSecret(_config["AzureAd:ClientSecret"])
//                .WithTenantId(_config["AzureAd:TenantId"])
//                .Build();
//        }

//        public async Task<string> GetAccessTokenAsync()
//        {
//            if (_authResult == null || _authResult.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5))
//            {
//                _authResult = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
//            }

//            return _authResult.AccessToken;
//        }
//    }
//}
