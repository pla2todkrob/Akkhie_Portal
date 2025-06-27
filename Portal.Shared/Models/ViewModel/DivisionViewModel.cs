using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel
{
    public class DivisionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "สายงาน")]
        [MaxLength(100)]
        [Required(ErrorMessage = "กรุณากรอกชื่อสายงาน")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "จำนวนฝ่าย")]
        public int TotalDepartment { get; set; }

        public List<DepartmentViewModel> DepartmentViewModels { get; set; } = [];
    }
}
