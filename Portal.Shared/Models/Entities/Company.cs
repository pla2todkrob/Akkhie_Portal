using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.Entities
{
    public class Company
    {
        public int Id { get; set; }

        [Display(Name = "บริษัท")]
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty!;

        [Display(Name = "ชื่อย่อ")]
        [MaxLength(10)]
        [Required]
        public string ShortName { get; set; } = string.Empty!;

        public ICollection<CompanyBranch> Branches { get; set; } = [];
        public ICollection<EmployeeCompanyAccess> EmployeeAccesses { get; set; } = [];
    }
}
