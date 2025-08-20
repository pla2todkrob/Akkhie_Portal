namespace Portal.Shared.Models.DTOs.Shared
{
    public class FileUploadResultDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string UploadPath { get; set; } = string.Empty;
    }
}
