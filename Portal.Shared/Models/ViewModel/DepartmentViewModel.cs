using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ฝ่าย")]
        [MaxLength(100)]
        [Required(ErrorMessage = "กรุณากรอกชื่อฝ่าย")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "บริษัท")]
        [Required(ErrorMessage = "กรุณาเลือกบริษัท")]
        public int CompanyId { get; set; }

        [Display(Name = "สายงาน")]
        [Required(ErrorMessage = "กรุณาเลือกสายงาน")]
        public int DivisionId { get; set; }

        [Display(Name = "สายงาน")]
        public string? DivisionName { get; set; }

        [Display(Name = "บริษัท")]
        public string? CompanyName { get; set; }

        [Display(Name = "จำนวนแผนก")]
        public int TotalSection { get; set; }

        public List<SectionViewModel> SectionViewModels { get; set; } = [];
    }
}
