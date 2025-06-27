using Portal.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class EmployeeCompanyAccess
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        public int? CompanyBranchId { get; set; }

        [ForeignKey("CompanyBranchId")]
        public CompanyBranch? CompanyBranch { get; set; }

        [Display(Name = "ระดับการเข้าถึง")]
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;

        [Display(Name = "วันที่มอบสิทธิ์")]
        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "วันที่สิ้นสุดสิทธิ์")]
        public DateTime? ExpiryDate { get; set; }

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
    }
}
