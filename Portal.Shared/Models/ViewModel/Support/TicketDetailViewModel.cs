using System;
using System.Collections.Generic;

namespace Portal.Shared.Models.ViewModel.Support
{
    public class TicketDetailViewModel
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string RequestType { get; set; }
        public string ReportedBy { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<HistoryItem> History { get; set; } = new();
        public List<AttachmentItem> Attachments { get; set; } = new();
        public Guid ReportedById { get; set; }
        public bool IsOwner { get; set; }

        public class HistoryItem
        {
            public string ActionDescription { get; set; }
            public string? Comment { get; set; }
            public string ActionBy { get; set; }
            public DateTime ActionDate { get; set; }
        }

        public class AttachmentItem
        {
            public string FileName { get; set; }
            public string FileUrl { get; set; }
            public string FileSizeDisplay { get; set; }
        }
    }
}
