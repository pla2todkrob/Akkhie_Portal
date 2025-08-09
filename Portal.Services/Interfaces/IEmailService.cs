using Portal.Shared.Models.Entities.Support;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    /// <summary>
    /// Interface สำหรับบริการส่งอีเมล
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// ส่งอีเมลแจ้งเตือนเมื่อมี Ticket ใหม่ถูกสร้าง
        /// </summary>
        /// <param name="ticket">ข้อมูล Ticket ที่สร้างใหม่</param>
        Task SendNewTicketNotificationAsync(SupportTicket ticket);

        /// <summary>
        /// (เพิ่มใหม่) ส่งอีเมลแจ้งเตือนเมื่อ Ticket ได้รับการแก้ไขเรียบร้อยแล้ว
        /// </summary>
        /// <param name="ticket">ข้อมูล Ticket ที่ถูกแก้ไข</param>
        Task SendTicketResolvedNotificationAsync(SupportTicket ticket);

        /// <summary>
        /// (เพิ่มใหม่) Method กลางสำหรับส่งอีเมล
        /// </summary>
        /// <param name="toEmails">รายชื่ออีเมลผู้รับ</param>
        /// <param name="subject">หัวข้ออีเมล</param>
        /// <param name="body">เนื้อหาอีเมล (HTML)</param>
        /// <param name="ccEmails">รายชื่ออีเมลสำหรับ CC (ถ้ามี)</param>
        /// <param name="bccEmails">รายชื่ออีเมลสำหรับ BCC (ถ้ามี)</param>
        Task SendEmailAsync(List<string> toEmails, string subject, string body, List<string>? ccEmails = null, List<string>? bccEmails = null);
    }
}
