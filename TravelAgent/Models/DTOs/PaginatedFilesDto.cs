namespace TravelAgent.Models.DTOs
{
    public class PaginatedFilesDto
    {
        public List<FileItemDto> Files { get; set; } = new();
        public string? NextLink { get; set; }
    }
}
