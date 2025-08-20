using Microsoft.Extensions.Options;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Models
{
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

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileEntity = new UploadedFile
                    {
                        FileName = uniqueFileName,
                        OriginalFileName = originalFileName,
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
