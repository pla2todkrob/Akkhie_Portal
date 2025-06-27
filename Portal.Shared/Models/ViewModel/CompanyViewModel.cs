using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel
{
    public class CompanyViewModel
    {
        public int Id { get; set; }

        [Display(Name = "บริษัท")]
        [MaxLength(100, ErrorMessage = "ชื่อบริษัทต้องไม่เกิน 100 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อบริษัท")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ชื่อย่อ")]
        [MaxLength(10, ErrorMessage = "ชื่อย่อต้องไม่เกิน 10 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อย่อบริษัท")]
        public string ShortName { get; set; } = string.Empty;

        [Display(Name = "จำนวนสาขา")]
        public int TotalBranch { get; set; }

        public List<CompanyBranchViewModel> CompanyBranchViewModels { get; set; } = [new CompanyBranchViewModel()];
    }
}
