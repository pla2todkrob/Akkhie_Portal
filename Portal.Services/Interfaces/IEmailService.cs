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
    }
}
