using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.ViewModel
{
    public class SectionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "แผนก")]
        [MaxLength(100)]
        [Required(ErrorMessage = "กรุณากรอกชื่อแผนก")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ฝ่าย")]
        [Required(ErrorMessage = "กรุณาเลือกฝ่าย")]
        public int DepartmentId { get; set; }

        [Display(Name = "ฝ่าย")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "สายงาน")]
        [Required(ErrorMessage = "กรุณาเลือกสายงาน")]
        public int DivisionId { get; set; }

        [Display(Name = "สายงาน")]
        public string DivisionName { get; set; } = string.Empty;
    }
}
