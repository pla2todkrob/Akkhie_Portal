using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.ViewModel
{
    public class CompanyBranchViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ชื่อสาขา")]
        [MaxLength(100, ErrorMessage = "ชื่อสาขาต้องไม่เกิน 100 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อสาขา")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "รหัสสาขา")]
        [MaxLength(10, ErrorMessage = "รหัสสาขาต้องไม่เกิน 10 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกรหัสสาขา")]
        public string BranchCode { get; set; } = string.Empty;

        public int CompanyId { get; set; }
    }
}
