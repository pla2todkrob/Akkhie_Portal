using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

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

        /// <summary>
        /// Gets a list of tickets reported by the current user.
        /// </summary>
        /// <returns>A list of tickets for the current user.</returns>
        Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync();
    }
}
