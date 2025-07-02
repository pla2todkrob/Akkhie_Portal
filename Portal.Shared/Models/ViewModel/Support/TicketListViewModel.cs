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

        [Display(Name = "วันที่แจ้ง")]
        public DateTime CreatedAt { get; set; }
    }
}
