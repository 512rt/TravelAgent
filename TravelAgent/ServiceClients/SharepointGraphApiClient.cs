using System.Net.Http.Headers;
using System.Text.Json;
using TravelAgent.Models.DTOs;
using TravelAgent.ServiceClients.Interfaces;
using TravelAgent.Services.Interfaces;

namespace TravelAgent.ServiceClients
{
    public class SharepointGraphApiClient : ISharepointGraphApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IGraphAuthProvider _authProvider;

        public SharepointGraphApiClient(IHttpClientFactory httpClientFactory, IGraphAuthProvider authProvider)
        {
            _httpClient = httpClientFactory.CreateClient("SPGraphAPIClient");
            _authProvider = authProvider;
        }

        public async Task<string> GetSiteDetailsAsync(string siteName)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/sites/rastech905.sharepoint.com:/sites/{siteName}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            
            return json;
        }

        public async Task<string?> GetDriveIdAsync(string siteId)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/sites/{siteId}/drives");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("value").EnumerateArray().FirstOrDefault().GetProperty("id").GetString();
        }

        public async Task<IEnumerable<FileItemDto>> GetFilesInRootAsync(string siteId, string driveId)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root/children");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var files = doc.RootElement
                   .GetProperty("value")
                   .EnumerateArray()
                   .Select(f => new FileItemDto
                   {
                       Name = f.GetProperty("name").GetString(),
                       Id = f.GetProperty("id").GetString(),
                       DownloadUrl = f.TryGetProperty("@microsoft.graph.downloadUrl", out var downloadUrlProp)
                           ? downloadUrlProp.GetString()
                           : null,
                       WebUrl = f.GetProperty("webUrl").GetString(),
                       Size = f.GetProperty("size").GetInt64()
                   })
                   .ToList();
            return files;
        }

        public async Task<PaginatedFilesDto> GetFilesPagedAsync(string siteId, string driveId, int pageSize, string? nextLink)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestUrl = string.IsNullOrEmpty(nextLink)
                ? $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root/children?$top={pageSize}"
                : nextLink;

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var files = doc.RootElement.GetProperty("value").EnumerateArray()
                .Select(f => new FileItemDto
                {
                    Name = f.GetProperty("name").GetString(),
                    Id = f.GetProperty("id").GetString(),
                    DownloadUrl = f.TryGetProperty("@microsoft.graph.downloadUrl", out var downloadUrlProp)
                        ? downloadUrlProp.GetString()
                        : null,
                    WebUrl = f.GetProperty("webUrl").GetString(),
                    Size = f.GetProperty("size").GetInt64()
                }).ToList();

            var nextPageLink = doc.RootElement.TryGetProperty("@odata.nextLink", out var nextProp)
                ? nextProp.GetString()
                : null;

            return new PaginatedFilesDto
            {
                Files = files,
                NextLink = nextPageLink
            };
        }


        public async Task<IEnumerable<string>> GetFilesInFolderAsync(string siteId, string driveId, string folderName)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:/{folderName}:/children");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("value")
                       .EnumerateArray()
                       .Select(el => el.GetProperty("name").GetString()!)
                       .ToList();
        }

        public async Task<bool> UploadDocumentAsync(string siteId, string driveId, string fileName, Stream fileStream)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var uploadUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:/{fileName}:/content";
            var content = new StreamContent(fileStream);

            var response = await _httpClient.PutAsync(uploadUrl, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UploadDocumentToFolderAsync(string siteId, string driveId, string relativePath, string fileName, Stream fileStream)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var uploadUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:/{relativePath}/{fileName}:/content";
            var content = new StreamContent(fileStream);

            var response = await _httpClient.PutAsync(uploadUrl, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDocumentAsync(string siteId, string driveId, string fileName)
        {
            var token = await _authProvider.GetGraphAPIAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:/{fileName}";
            var response = await _httpClient.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
    }
}
