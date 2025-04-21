using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TravelAgent.ServiceClients.Interfaces;

namespace TravelAgent.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SharepointGraphApiController : ControllerBase
    {
        private readonly ISharepointGraphApiClient _spGraphApiClient;

        public SharepointGraphApiController(ISharepointGraphApiClient spGraphApiClient)
        {
            _spGraphApiClient = spGraphApiClient;
        }

        [HttpGet("sites/{siteName}")]
        public async Task<IActionResult> GetSiteDetails(string siteName)
        {
            try
            {
                var result = await _spGraphApiClient.GetSiteDetailsAsync(siteName);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving site details: {ex.Message}");
            }
        }

        [HttpGet("drive-id/{siteId}")]
        public async Task<IActionResult> GetDriveId(string siteId)
        {
            try
            {
                var driveId = await _spGraphApiClient.GetDriveIdAsync(siteId);
                if (string.IsNullOrEmpty(driveId))
                    return NotFound("Drive ID not found");

                return Ok(new { driveId });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving drive ID: {ex.Message}");
            }
        }

        [HttpGet("{siteId}/{driveId}/files")]
        public async Task<IActionResult> GetFilesInRoot(string siteId, string driveId)
        {
            try
            {
                var files = await _spGraphApiClient.GetFilesInRootAsync(siteId, driveId);
                return Ok(files);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving files: {ex.Message}");
            }
        }

        [HttpGet("{siteId}/{driveId}/folder/{folderName}")]
        public async Task<IActionResult> GetFilesInFolder(string siteId, string driveId, string folderName)
        {
            try
            {
                var files = await _spGraphApiClient.GetFilesInFolderAsync(siteId, driveId, folderName);
                return Ok(files);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving folder contents: {ex.Message}");
            }
        }

        [HttpPost("{siteId}/{driveId}/upload")]
        public async Task<IActionResult> UploadFile(string siteId, string driveId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is missing or empty.");

            try
            {
                using var stream = file.OpenReadStream();
                var success = await _spGraphApiClient.UploadDocumentAsync(siteId, driveId, file.FileName, stream);
                return success ? Ok("File uploaded successfully.") : StatusCode(500, "Upload failed.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpPost("{siteId}/{driveId}/upload-to-folder")]
        public async Task<IActionResult> UploadToFolder(string siteId, string driveId, string relativePath, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is missing or empty.");

            try
            {
                using var stream = file.OpenReadStream();
                var success = await _spGraphApiClient.UploadDocumentToFolderAsync(siteId, driveId, relativePath, file.FileName, stream);
                return success ? Ok($"{file.FileName} uploaded successfully.") : StatusCode(500, $"Failed to upload {file.FileName}.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading to folder: {ex.Message}");
            }
        }

        [HttpDelete("{siteId}/{driveId}/delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string siteId, string driveId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Filename must be provided.");

            try
            {
                var success = await _spGraphApiClient.DeleteDocumentAsync(siteId, driveId, fileName);
                return success ? Ok($"{fileName} was deleted successfully.") : StatusCode(500, "Deletion failed.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                                  $"HTTP error: {(int?)ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }
    }
}
