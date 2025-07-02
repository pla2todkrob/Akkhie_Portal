using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Services.Models
{
    public class SupportTicketService(PortalDbContext context, ICurrentUserService currentUserService) : ISupportTicketService
    {
        private readonly PortalDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request)
        {
            if (_currentUserService.UserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var ticket = new SupportTicket
            {
                TicketNumber = await GenerateNextTicketNumberAsync(),
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Priority = request.Priority,
                AssetId = request.AssetId,
                Status = Shared.Enums.Support.TicketStatus.Open,
                RequestType = Shared.Enums.Support.TicketRequestType.Issue, // Defaulting to 'Issue' for now
                ReportedByEmployeeId = _currentUserService.UserId.Value,
                CreatedAt = DateTime.UtcNow,
            };

            // Add an initial history entry
            ticket.History.Add(new SupportTicketHistory
            {
                EmployeeId = _currentUserService.UserId.Value,
                ActionDescription = "Created Ticket",
                Comment = "Ticket has been created.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            return ticket;
        }

        public async Task<SupportTicket?> GetTicketByIdAsync(int ticketId)
        {
            // For now, allow any authenticated user to see any ticket for testing.
            // In a real scenario, we would add logic to check if the current user is the reporter or an IT admin.
            return await _context.SupportTickets
                .Include(t => t.Category)
                .Include(t => t.ReportedByEmployee)
                    .ThenInclude(e => e.EmployeeDetail)
                .Include(t => t.AssignedToEmployee)
                    .ThenInclude(e => e!.EmployeeDetail)
                .Include(t => t.RelatedAsset)
                .Include(t => t.History)
                    .ThenInclude(h => h.Employee)
                        .ThenInclude(e => e.EmployeeDetail)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync()
        {
            if (_currentUserService.UserId == null)
            {
                return [];
            }

            var userId = _currentUserService.UserId.Value;

            return await _context.SupportTickets
                .Where(t => t.ReportedByEmployeeId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Generates a new unique ticket number based on the current year and count.
        /// Format: TICKET-YYYY-NNNNN
        /// </summary>
        private async Task<string> GenerateNextTicketNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var yearPrefix = $"TICKET-{year}-";

            var lastTicketCount = await _context.SupportTickets
                .CountAsync(t => t.TicketNumber.StartsWith(yearPrefix));

            var nextId = lastTicketCount + 1;
            return $"{yearPrefix}{nextId:D5}"; // D5 pads with leading zeros, e.g., 00001
        }
    }
}
