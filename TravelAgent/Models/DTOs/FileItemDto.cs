namespace TravelAgent.Models.DTOs
{
    public class FileItemDto
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string? DownloadUrl { get; set; }
        public string WebUrl { get; set; }
        public long Size { get; set; }
    }
}
