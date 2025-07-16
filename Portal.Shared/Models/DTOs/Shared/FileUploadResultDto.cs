namespace Portal.Shared.Models.DTOs.Shared
{
    /// <summary>
    /// DTO สำหรับผลลัพธ์การอัพโหลดไฟล์
    /// </summary>
    public class FileUploadResultDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string UploadPath { get; set; }
    }
}
