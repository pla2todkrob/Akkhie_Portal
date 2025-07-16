using Portal.Shared.Enums.Support;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Support
{
    public class TicketActionRequest
    {
        [Required]
        public int TicketId { get; set; }

        public TicketPriority? Priority { get; set; }

        public Guid? AssignedToEmployeeId { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
        public List<int> UploadedFileIds { get; set; } = new List<int>();
    }
}
