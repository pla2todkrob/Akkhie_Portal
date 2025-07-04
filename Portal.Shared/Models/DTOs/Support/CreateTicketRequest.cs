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
    }
}
