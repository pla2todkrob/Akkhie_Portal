﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<int> UploadedFileIds { get; set; } = new List<int>();
    }
}
