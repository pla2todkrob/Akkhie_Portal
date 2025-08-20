using Portal.Shared.Models.Entities.Support;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendNewTicketNotificationAsync(SupportTicket ticket, int queuePosition);

        Task SendTicketResolvedNotificationAsync(SupportTicket ticket);

        Task SendEmailAsync(List<string> toEmails, string subject, string body, List<string>? ccEmails = null, List<string>? bccEmails = null);
    }
}
