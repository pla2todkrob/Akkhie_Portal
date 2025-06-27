using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class CompanyBranch
    {
        public int Id { get; set; }

        [Display(Name = "ชื่อสาขา")]
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty!;

        [Display(Name = "รหัสสาขา")]
        [MaxLength(10)]
        public string BranchCode { get; set; } = string.Empty!;

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; } = null!;

        public ICollection<EmployeeCompanyAccess> EmployeeAccesses { get; set; } = [];
    }
}
