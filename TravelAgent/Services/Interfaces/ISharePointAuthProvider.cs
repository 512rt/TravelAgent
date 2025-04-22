using Azure.Core;

namespace TravelAgent.Services.Interfaces
{
    public interface ISharePointAuthProvider
    {
        Task<string> GetGraphAPIAccessTokenAsync();
        Task<string> GetSharePointAPIAccessTokenAsync();
        TokenCredential GetTokenCredential();
    }
}
