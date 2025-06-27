// Portal.Shared/Models/Entities/Employee.cs
using Portal.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "ชื่อบัญชี")]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Password Hash")]
        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [Display(Name = "ผู้ใช้ AD")]
        public bool IsAdUser { get; set; } = false;

        public int? DivisionId { get; set; }

        [ForeignKey("DivisionId")]
        public Division? Division { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? SectionId { get; set; }

        [ForeignKey("SectionId")]
        public Section? Section { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;

        public bool IsSystemRole { get; set; } = false;
        public ICollection<EmployeeCompanyAccess> EmployeeCompanyAccesses { get; set; } = [];

        [DisplayFormat(DataFormatString = "{0:g}")]
        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime CreatedAtLocalTime => CreatedAt.ToLocalTime();

        [Display(Name = "สถานะ")]
        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;

        public EmployeeDetail? EmployeeDetail { get; set; }

        public int? ProfilePictureId { get; set; }

        [ForeignKey("ProfilePictureId")]
        public UploadedFile? ProfilePicture { get; set; }

        public ICollection<UploadedFile> UploadedFiles { get; set; } = [];
        public ICollection<AuditLog> AuditLogs { get; set; } = [];
    }
}
