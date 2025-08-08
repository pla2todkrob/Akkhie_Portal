using Portal.Shared.Enums.Support;
using Portal.Shared.Models.Entities.IT_Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.Support
{
    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "หมายเลข Ticket")]
        [Required]
        [MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "หัวข้อ")]
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "รายละเอียด")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "ประเภท Ticket")]
        public TicketRequestType RequestType { get; set; }

        [Display(Name = "หมวดหมู่")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public SupportTicketCategory Category { get; set; } = null!;

        [Display(Name = "ทรัพย์สินที่เกี่ยวข้อง")]
        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public IT_Asset? RelatedAsset { get; set; }

        [Display(Name = "อุปกรณ์ที่ต้องการ")]
        public int? RequestedItemId { get; set; }
        [ForeignKey("RequestedItemId")]
        public IT_Item? RequestedItem { get; set; }

        [Display(Name = "ความสำคัญ")]
        public TicketPriority Priority { get; set; }

        [Display(Name = "สถานะ")]
        public TicketStatus Status { get; set; }

        [Display(Name = "ผู้แจ้ง")]
        public Guid ReportedByEmployeeId { get; set; }
        [ForeignKey("ReportedByEmployeeId")]
        public Employee ReportedByEmployee { get; set; } = null!;

        [Display(Name = "ผู้รับผิดชอบ")]
        public Guid? AssignedToEmployeeId { get; set; }
        [ForeignKey("AssignedToEmployeeId")]
        public Employee? AssignedToEmployee { get; set; }

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "วันที่อัปเดต")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "วันที่แก้ไขเสร็จ")]
        public DateTime? ResolvedAt { get; set; }

        public ICollection<SupportTicketHistory> History { get; set; } = [];
        public int? RelatedTicketId { get; set; }
        public virtual SupportTicket? RelatedTicket { get; set; }

        public virtual ICollection<SupportTicketFiles> SupportTicketFiles { get; set; } = [];
    }
}
