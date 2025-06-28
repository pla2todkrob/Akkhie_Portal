using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.DTOs.Support
{
    public class CreateTicketRequest
    {
        [Required(ErrorMessage = "กรุณาระบุหัวข้อ")]
        [MaxLength(255)]
        [Display(Name = "หัวข้อ")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุรายละเอียด")]
        [Display(Name = "รายละเอียดปัญหา")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
        [Display(Name = "หมวดหมู่")]
        public int CategoryId { get; set; }

        [Display(Name = "ระดับความสำคัญ")]
        public Enums.Support.TicketPriority Priority { get; set; } = Enums.Support.TicketPriority.Medium;

        [Display(Name = "ทรัพย์สินที่เกี่ยวข้อง (ถ้ามี)")]
        public int? AssetId { get; set; }

        // We will handle file upload separately and link it later.
        // public IFormFile? Attachment { get; set; }
    }
}
