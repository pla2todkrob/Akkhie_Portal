using Portal.Shared.Models.Entities.Support;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class UploadedFile
    {
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string ContentType { get; set; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string UploadPath { get; set; } = string.Empty;

        public DateTime UploadDateTime { get; set; }

        [Display(Name = "ขนาดไฟล์ (KB)")]
        public long FileSizeInKb => FileSize / 1024;

        public long FileSize { get; set; }

        public Guid UploadedByUserId { get; set; }

        public Employee UploadedByUser { get; set; } = null!;


        [NotMapped]
        public string FileSizeDisplay => FileSizeInKb > 1024
        ? $"{FileSizeInKb / 1024:N2} MB"
        : $"{FileSizeInKb:N2} KB";

        [NotMapped]
        public string? FileUrl { get; set; }
    }
}
