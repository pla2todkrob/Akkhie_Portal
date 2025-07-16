using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;
using Portal.Shared.Constants;
using System.Text;

namespace Portal.Services.Models
{
    public class SupportTicketService(PortalDbContext context, ICurrentUserService currentUserService, ILineMessagingService lineMessagingService) : ISupportTicketService
    {
        private readonly PortalDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request)
        {
            if (_currentUserService.UserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Find a default "General" or "Uncategorized" category for issues.
            // This should be seeded in the database.
            var defaultCategory = await _context.SupportTicketCategories
                                      .FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Issue && c.Name == "None")
                                  ?? await _context.SupportTicketCategories.FirstAsync(c => c.CategoryType == TicketCategoryType.Issue);

            if (defaultCategory == null)
            {
                throw new InvalidOperationException("ไม่พบหมวดหมู่เริ่มต้นสำหรับแจ้งปัญหา");
            }

            var ticket = new SupportTicket
            {
                TicketNumber = await GenerateNextTicketNumberAsync(),
                Title = request.Title,
                Description = request.Description,
                CategoryId = defaultCategory.Id, // Use default category
                Priority = TicketPriority.Medium, // Always start with Medium priority
                Status = TicketStatus.Open,
                RequestType = TicketRequestType.Issue,
                ReportedByEmployeeId = _currentUserService.UserId.Value,
                CreatedAt = DateTime.UtcNow,
                // AssetId is now null by default
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            // **อัปเดต** แก้ไขการเรียกใช้ Service แจ้งเตือน
            var createdTicket = await _context.SupportTickets
                .Include(t => t.ReportedByEmployee).ThenInclude(e => e.EmployeeDetail)
                .Include(t => t.ReportedByEmployee).ThenInclude(e => e.Section)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            if (createdTicket != null)
            {
                // เปลี่ยนจาก _lineNotifyService เป็น _lineMessagingService
                await lineMessagingService.SendTicketCreationNotificationAsync(createdTicket);
            }

            return ticket;
        }

        public async Task<TicketDetailViewModel?> GetTicketByIdAsync(int ticketId)
        {
            // For now, allow any authenticated user to see any ticket for testing.
            // In a real scenario, we would add logic to check if the current user is the reporter or an IT admin.
            var ticket = await _context.SupportTickets
                .Where(t => t.Id == ticketId)
                .Include(t => t.Category)
                .Include(t => t.ReportedByEmployee.EmployeeDetail)
                .Include(t => t.AssignedToEmployee!.EmployeeDetail)
                .Include(t => t.History)
                    .ThenInclude(h => h.Employee.EmployeeDetail)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return null;
            }

            return new TicketDetailViewModel
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status.GetDisplayName(),
                Priority = ticket.Priority.GetDisplayName(),
                Category = ticket.Category.Name,
                RequestType = ticket.RequestType.GetDisplayName(),
                ReportedBy = ticket.ReportedByEmployee.EmployeeDetail?.FullName ?? ticket.ReportedByEmployee.Username,
                AssignedTo = ticket.AssignedToEmployee?.EmployeeDetail?.FullName ?? ticket.AssignedToEmployee?.Username,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                History = ticket.History.OrderByDescending(h => h.CreatedAt).Select(h => new TicketDetailViewModel.HistoryItem
                {
                    ActionDescription = h.ActionDescription,
                    Comment = h.Comment,
                    ActionBy = h.Employee.EmployeeDetail?.FullName ?? h.Employee.Username,
                    ActionDate = h.CreatedAt
                }).ToList()
            };
        }
        public async Task<bool> AcceptTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.Status != TicketStatus.Open)
            {
                throw new InvalidOperationException("Ticket has already been accepted or is closed.");
            }

            var currentUser = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

            ticket.Status = TicketStatus.InProgress;
            ticket.Priority = request.Priority ?? ticket.Priority;
            ticket.AssignedToEmployeeId = request.AssignedToEmployeeId ?? currentUser;
            ticket.UpdatedAt = DateTime.UtcNow;

            var history = new SupportTicketHistory
            {
                TicketId = ticket.Id,
                EmployeeId = currentUser,
                ActionDescription = "รับงานและกำหนดความสำคัญ",
                Comment = $"กำหนดความสำคัญเป็น: {ticket.Priority.GetDisplayName()}. มอบหมายให้: {(await _context.Employees.Include(e => e.EmployeeDetail).FirstOrDefaultAsync(e => e.Id == ticket.AssignedToEmployeeId))?.EmployeeDetail?.FullName}",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketHistories.Add(history);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResolveTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            {
                throw new InvalidOperationException("Ticket has already been resolved or closed.");
            }

            var currentUser = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
            var category = await _context.SupportTicketCategories.FindAsync(request.CategoryId)
                           ?? throw new KeyNotFoundException("Category not found.");

            ticket.Status = TicketStatus.Resolved;
            ticket.CategoryId = category.Id;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            var history = new SupportTicketHistory
            {
                TicketId = ticket.Id,
                EmployeeId = currentUser,
                ActionDescription = "ดำเนินการแก้ไขเสร็จสิ้น",
                Comment = $"เปลี่ยนหมวดหมู่เป็น: '{category.Name}'. บันทึกการแก้ไข: {request.Comment}",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketHistories.Add(history);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync()
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return new List<TicketListViewModel>();
            }

            return await _context.SupportTickets
                .Where(t => t.ReportedByEmployeeId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status,
                    StatusName = t.Status.GetDisplayName(),
                    CreatedAt = t.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync(TicketCategoryType categoryType)
        {
            return await _context.SupportTicketCategories
                .Where(c => c.CategoryType == categoryType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SupportTicket> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate stock availability
                foreach (var item in request.Items)
                {
                    var stockItem = await _context.IT_Stocks.FirstOrDefaultAsync(s => s.ItemId == item.ItemId);
                    if (stockItem == null || stockItem.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"สินค้า ID: {item.ItemId} มีไม่เพียงพอในสต็อก");
                    }
                }

                // 2. Deduct stock quantities
                foreach (var item in request.Items)
                {
                    var stockItem = await _context.IT_Stocks.FirstAsync(s => s.ItemId == item.ItemId);
                    stockItem.Quantity -= item.Quantity;
                    _context.IT_Stocks.Update(stockItem);
                }

                // 3. Create ticket description
                var descriptionBuilder = new StringBuilder("รายการเบิกอุปกรณ์:\n");
                foreach (var item in request.Items)
                {
                    var dbItem = await _context.IT_Items.FindAsync(item.ItemId);
                    descriptionBuilder.AppendLine($"- {dbItem?.Name ?? "N/A"}: {item.Quantity} ชิ้น");
                }

                // 4. Find the "Withdrawal" category
                var category = await _context.SupportTicketCategories
                                   .FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request)
                               ?? throw new InvalidOperationException("ไม่พบหมวดหมู่สำหรับการเบิกอุปกรณ์");

                // 5. Create the ticket
                var ticket = new SupportTicket
                {
                    TicketNumber = await GenerateNextTicketNumberAsync(),
                    Title = "คำขอเบิกอุปกรณ์จากสต็อก",
                    Description = descriptionBuilder.ToString(),
                    CategoryId = category.Id,
                    Priority = TicketPriority.Medium,
                    Status = TicketStatus.Open,
                    RequestType = TicketRequestType.Request,
                    ReportedByEmployeeId = userId,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.SupportTickets.Add(ticket);

                // 6. Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ticket;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception to be handled by the controller
            }
        }

        public async Task<SupportTicket> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

            // Find the "Purchase Request" category
            var category = await context.SupportTicketCategories
                               .FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request && c.Name == "Purchase Request")
                           ?? throw new InvalidOperationException("ไม่พบหมวดหมู่สำหรับการขอจัดซื้อ");

            // Build a structured description
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine($"**อุปกรณ์:** {request.ItemName}");
            descriptionBuilder.AppendLine($"**จำนวน:** {request.Quantity} ชิ้น");
            if (!string.IsNullOrWhiteSpace(request.Specification))
            {
                descriptionBuilder.AppendLine($"**สเปค/รายละเอียด:** {request.Specification}");
            }
            descriptionBuilder.AppendLine("\n---\n");
            descriptionBuilder.AppendLine($"**เหตุผลในการขอ:**\n{request.Reason}");

            var ticket = new SupportTicket
            {
                TicketNumber = await GenerateNextTicketNumberAsync(),
                Title = $"ขอจัดซื้อ: {request.ItemName}",
                Description = descriptionBuilder.ToString(),
                CategoryId = category.Id,
                Priority = TicketPriority.Medium, // Default priority for purchase requests
                Status = TicketStatus.Open,
                RequestType = TicketRequestType.Request,
                ReportedByEmployeeId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            context.SupportTickets.Add(ticket);
            await context.SaveChangesAsync();

            return ticket;
        }

        public async Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync()
        {
            return await context.SupportTickets
                .AsNoTracking()
                .Include(t => t.ReportedByEmployee)
                    .ThenInclude(e => e.Department)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status,
                    StatusName = t.Status.GetDisplayName(),
                    CreatedAt = t.CreatedAt,
                    ReportedBy = t.ReportedByEmployee.EmployeeDetail != null ? t.ReportedByEmployee.EmployeeDetail.FullName : t.ReportedByEmployee.Username,
                    DepartmentName = t.ReportedByEmployee.Department != null ? t.ReportedByEmployee.Department.Name : "N/A"
                })
                .ToListAsync();
        }
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
