using Azure.Core;

namespace TravelAgent.Services.Interfaces
{
    public interface IGraphAuthProvider
    {
        Task<string> GetGraphAPIAccessTokenAsync();
        Task<string> GetSharePointAPIAccessTokenAsync();
        TokenCredential GetTokenCredential();
    }
}
