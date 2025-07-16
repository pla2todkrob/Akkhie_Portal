using Portal.Shared.Models.Entities.Support;

namespace Portal.Services.Interfaces
{
    public interface ILineMessagingService
    {
        Task SendPushMessageAsync(string to, string message);
        Task SendTicketCreationNotificationAsync(SupportTicket ticket);
    }
}
