using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.Support
{
    public class SupportTicketHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public SupportTicket Ticket { get; set; } = null!;

        [Required]
        public Guid EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Display(Name = "การกระทำ")]
        [MaxLength(255)]
        public string ActionDescription { get; set; } = string.Empty;

        [Display(Name = "ความคิดเห็น")]
        public string? Comment { get; set; }

        public int? FileAttachmentId { get; set; }
        [ForeignKey("FileAttachmentId")]
        public UploadedFile? FileAttachment { get; set; }

        [Display(Name = "วันที่")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
