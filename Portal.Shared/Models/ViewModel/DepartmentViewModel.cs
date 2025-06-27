using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.ViewModel
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ฝ่าย")]
        [MaxLength(100)]
        [Required(ErrorMessage = "กรุณากรอกชื่อฝ่าย")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "สายงาน")]
        [Required(ErrorMessage = "กรุณาเลือกสายงาน")]
        public int DivisionId { get; set; }

        [Display(Name = "สายงาน")]
        public string DivisionName { get; set; } = string.Empty;

        [Display(Name = "จำนวนแผนก")]
        public int TotalSection { get; set; }

        public List<SectionViewModel> SectionViewModels { get; set; } = [];
    }
}
