using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel
{
    public class DivisionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสายงาน")]
        [Display(Name = "ชื่อสายงาน")]
        public string Name { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกบริษัท")]
        [Display(Name = "บริษัท")]
        public int CompanyId { get; set; }

        [Display(Name = "บริษัท")]
        public string? CompanyName { get; set; }

        [Display(Name = "จำนวนฝ่าย")]
        public int TotalDepartment { get; set; }

        public List<DepartmentViewModel> DepartmentViewModels { get; set; } = [];
    }
}
