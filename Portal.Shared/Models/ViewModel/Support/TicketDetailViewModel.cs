using Portal.Shared.Enums.Support;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel.Support
{
    public class TicketDetailViewModel
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<HistoryItem> History { get; set; } = [];

        public class HistoryItem
        {
            public string ActionDescription { get; set; } = string.Empty;
            public string? Comment { get; set; }
            public string ActionBy { get; set; } = string.Empty;
            public DateTime ActionDate { get; set; }
        }
    }
}