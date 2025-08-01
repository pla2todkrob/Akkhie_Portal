using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    /// <summary>
    /// Interface สำหรับจัดการระบบ Support Ticket ทั้งหมด
    /// </summary>
    public interface ISupportTicketService
    {
        Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request);
        Task<TicketDetailViewModel?> GetTicketByIdAsync(int ticketId);
        Task<bool> AcceptTicketAsync(TicketActionRequest request);
        Task<bool> ResolveTicketAsync(TicketActionRequest request);
        Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync(TicketCategoryType categoryType);
        Task<SupportTicket> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request);
        Task<SupportTicket> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request);
        Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
