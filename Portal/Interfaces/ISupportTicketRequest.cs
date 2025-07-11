// File: Portal/Interfaces/ISupportTicketRequest.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Interfaces
{
    public interface ISupportTicketRequest
    {
        Task<ApiResponse<SupportTicket>> CreateTicketAsync(CreateTicketRequest model);
        Task<ApiResponse<IEnumerable<SupportTicketCategory>>> GetCategoriesAsync(string categoryType);
        Task<ApiResponse<IEnumerable<TicketListViewModel>>> GetMyTicketsAsync();
        Task<ApiResponse<SupportTicket>> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request);
        Task<ApiResponse<SupportTicket>> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request);
        Task<ApiResponse<IEnumerable<TicketListViewModel>>> GetAllTicketsAsync();
        Task<ApiResponse<TicketDetailViewModel>> GetTicketDetailsAsync(int ticketId);
        Task<ApiResponse<bool>> AcceptTicketAsync(TicketActionRequest request);
        Task<ApiResponse<bool>> ResolveTicketAsync(TicketActionRequest request);
    }
}