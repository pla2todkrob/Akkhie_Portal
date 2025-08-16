using Portal.Shared.Enums.Support;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel.Support
{
    public class TicketListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "หมายเลข")]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "หัวข้อ")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "สถานะ")]
        public TicketStatus Status { get; set; }

        [Display(Name = "สถานะ")]
        public string StatusName { get; set; } = string.Empty;

        [Display(Name = "วันที่แจ้ง")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "ผู้แจ้ง")]
        public string? ReportedBy { get; set; }

        [Display(Name = "ฝ่าย")]
        public string? DepartmentName { get; set; }
        [Display(Name = "หมวดหมู่")]
        public string CategoryName { get; set; }
    }
}
