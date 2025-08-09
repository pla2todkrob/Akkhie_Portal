using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    /// <summary>
    /// Service สำหรับจัดการไฟล์
    /// </summary>
    public class FileService(PortalDbContext context, IOptions<FileSettings> fileSettings, ICurrentUserService currentUserService) : IFileService
    {
        private readonly string _uploadPath = fileSettings.Value.UploadPath;

        public async Task<List<UploadedFile>> UploadFilesAsync(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("No files provided for upload.", nameof(files));
            }

            var uploadedFileEntities = new List<UploadedFile>();
            var yearMonthFolder = DateTime.UtcNow.ToString("yyyy\\MM");
            var physicalDirectory = Path.Combine(_uploadPath, yearMonthFolder);

            // สร้าง Directory ถ้ายังไม่มี
            if (!Directory.Exists(physicalDirectory))
            {
                Directory.CreateDirectory(physicalDirectory);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var originalFileName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(originalFileName);
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var physicalPath = Path.Combine(physicalDirectory, uniqueFileName);

                    // บันทึกไฟล์ลง Server
                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // สร้าง Entity สำหรับบันทึกลง Database ให้ตรงกับ Model ที่ให้มา
                    var fileEntity = new UploadedFile
                    {
                        FileName = uniqueFileName, // ชื่อไฟล์ที่เก็บใน Server
                        OriginalFileName = originalFileName, // ชื่อไฟล์ดั้งเดิม
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        UploadPath = Path.Combine(yearMonthFolder, uniqueFileName).Replace('\\', '/'),
                        UploadDateTime = DateTime.UtcNow,
                        UploadedByUserId = currentUserService.UserId!.Value
                    };

                    context.UploadedFiles.Add(fileEntity);
                    uploadedFileEntities.Add(fileEntity);
                }
            }

            await context.SaveChangesAsync();

            return uploadedFileEntities;
        }
    }
}
