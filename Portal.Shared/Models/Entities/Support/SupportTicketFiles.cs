using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.Support
{
    public class SupportTicketFiles
    {
        public int Id { get; set; }
        public int SupportTicketId { get; set; }
        [ForeignKey("SupportTicketId")]
        public SupportTicket SupportTicket { get; set; }
        public int UploadedFileId { get; set; }
        [ForeignKey("UploadedFileId")]
        public UploadedFile UploadedFile { get; set; }
    }
}
