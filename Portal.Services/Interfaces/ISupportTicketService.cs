using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;

namespace Portal.Services.Interfaces
{
    public interface ISupportTicketService
    {
        /// <summary>
        /// Creates a new support ticket.
        /// </summary>
        /// <param name="request">The ticket creation request from the user.</param>
        /// <returns>The newly created SupportTicket entity.</returns>
        Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request);

        /// <summary>
        /// Gets a ticket by its ID, ensuring the current user has permission to view it.
        /// </summary>
        /// <param name="ticketId">The ID of the ticket to retrieve.</param>
        /// <returns>The SupportTicket entity or null if not found or not authorized.</returns>
        Task<SupportTicket?> GetTicketByIdAsync(int ticketId);

        // เราจะเพิ่มเมธอดอื่นๆ เช่น การอัปเดต, การดึงรายการทั้งหมด, ในภายหลัง
    }
}
