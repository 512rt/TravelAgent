using TravelAgent.Models.DTOs;

namespace TravelAgent.ServiceClients.Interfaces
{
    public interface ISharepointGraphApiClient
    {
        Task<string> GetSiteDetailsAsync(string siteName);
        Task<string?> GetDriveIdAsync(string siteId);
        Task<IEnumerable<FileItemDto>> GetFilesInRootAsync(string siteId, string driveId);
        Task<PaginatedFilesDto> GetFilesPagedAsync(string siteId, string driveId, int pageSize, string? nextLink);
        Task<IEnumerable<string>> GetFilesInFolderAsync(string siteId, string driveId, string folderName);
        Task<bool> UploadDocumentAsync(string siteId, string driveId, string fileName, Stream fileStream);
        Task<bool> UploadDocumentToFolderAsync(string siteId, string driveId, string relativePath, string fileName, Stream fileStream);
        Task<bool> DeleteDocumentAsync(string siteId, string driveId, string fileName);
    }
}
