using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Support
{
    public class CreateTicketRequest
    {
        [Required(ErrorMessage = "กรุณากรอกหัวข้อปัญหา")]
        [StringLength(200, ErrorMessage = "หัวข้อต้องไม่เกิน 200 ตัวอักษร")]
        [Display(Name = "หัวข้อปัญหา")]
        public string Title { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรายละเอียดของปัญหา")]
        [Display(Name = "รายละเอียด")]
        public string Description { get; set; }

        [Display(Name = "อ้างอิง Ticket เก่า")]
        public int? RelatedTicketId { get; set; }

        public List<IFormFile> UploadedFiles { get; set; } = [];
    }
}
