using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Interfaces
{
    public interface ISupportTicketRequest
    {
        Task<ApiResponse<SupportTicket>> CreateTicketAsync(CreateTicketRequest model);
        Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync(string categoryType);
        Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync();
        Task<IEnumerable<TicketListViewModel>> GetMyClosedTicketsAsync();
        Task<ApiResponse<SupportTicket>> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request);
        Task<ApiResponse<SupportTicket>> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request);
        Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync();
        Task<TicketDetailViewModel> GetTicketDetailsAsync(int ticketId);
        Task<ApiResponse<bool>> AcceptTicketAsync(TicketActionRequest request);
        Task<ApiResponse<bool>> ResolveTicketAsync(TicketActionRequest request);
        Task<ApiResponse<bool>> CloseTicketAsync(TicketActionRequest request);
        Task<ApiResponse<bool>> CancelTicketAsync(TicketActionRequest request);
        Task<ApiResponse<bool>> RejectTicketAsync(TicketActionRequest request);
    }
}