using Portal.Shared.Models.Entities;

namespace Portal.Services.Interfaces
{
    /// <summary>
    /// Interface สำหรับจัดการไฟล์
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// อัพโหลดไฟล์หลายไฟล์พร้อมกัน
        /// </summary>
        /// <param name="files">รายการไฟล์ที่ต้องการอัพโหลด</param>
        /// <returns>รายการข้อมูลไฟล์ที่บันทึกในฐานข้อมูล</returns>
        Task<List<UploadedFile>> UploadFilesAsync(List<IFormFile> files);
    }
}
