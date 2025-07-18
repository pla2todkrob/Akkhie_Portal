using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel
{
    public class DivisionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสายงาน")]
        [Display(Name = "ชื่อสายงาน")]
        public string Name { get; set; }

        // เพิ่ม Property นี้เพื่อรับค่า CompanyId จากฟอร์ม
        [Required(ErrorMessage = "กรุณาเลือกบริษัท")]
        [Display(Name = "บริษัท")]
        public int CompanyId { get; set; }

        // เพิ่ม Property นี้เพื่อใช้แสดงชื่อบริษัทในหน้า Index
        [Display(Name = "บริษัท")]
        public string CompanyName { get; set; }

        [Display(Name = "จำนวนฝ่าย")]
        public int TotalDepartment { get; set; }

        public List<DepartmentViewModel> DepartmentViewModels { get; set; } = new List<DepartmentViewModel>();
    }
}
