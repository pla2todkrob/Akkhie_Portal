using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Enums;
using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class SupportTicketService(
        PortalDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<SupportTicketService> logger) : ISupportTicketService
    {
        public async Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request)
        {
            var userId = currentUserService.UserId!.Value;
            var defaultCategory = await context.SupportTicketCategories
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Issue)
                                  ?? throw new InvalidOperationException("ไม่พบหมวดหมู่เริ่มต้นสำหรับแจ้งปัญหา (Issue)");

            var newTicket = new SupportTicket
            {
                TicketNumber = await GenerateNewTicketNumberAsync(),
                Title = request.Title,
                Description = request.Description,
                RequestType = TicketRequestType.Issue,
                CategoryId = defaultCategory.Id,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open,
                ReportedByEmployeeId = userId,
                CreatedAt = DateTime.UtcNow,
                RelatedTicketId = request.RelatedTicketId
            };

            context.SupportTickets.Add(newTicket);
            await context.SaveChangesAsync();

            await AssociateFilesToTicket(newTicket.Id, request.UploadedFileIds);
            CreateHistoryEntry(newTicket.Id, userId, "สร้าง Ticket ใหม่", "ระบบสร้าง Ticket อัตโนมัติ");
            await context.SaveChangesAsync();

            _ = emailService.SendNewTicketNotificationAsync(newTicket);
            logger.LogInformation("Successfully created Ticket #{TicketNumber} by user {UserId}", newTicket.TicketNumber, userId);

            return newTicket;
        }

        public async Task<TicketDetailViewModel?> GetTicketByIdAsync(int ticketId)
        {
            var ticket = await context.SupportTickets
                .AsNoTracking()
                .Where(t => t.Id == ticketId)
                .Include(t => t.Category)
                .Include(t => t.ReportedByEmployee).ThenInclude(e => e.EmployeeDetail)
                .Include(t => t.AssignedToEmployee).ThenInclude(e => e!.EmployeeDetail)
                .Include(t => t.History).ThenInclude(h => h.Employee).ThenInclude(e => e.EmployeeDetail)
                .Include(t => t.UploadedFiles)
                .FirstOrDefaultAsync();

            if (ticket == null) return null;

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
                AssignedTo = ticket.AssignedToEmployee?.EmployeeDetail?.FullName,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                History = ticket.History.OrderByDescending(h => h.CreatedAt).Select(h => new TicketDetailViewModel.HistoryItem
                {
                    ActionDescription = h.ActionDescription,
                    Comment = h.Comment,
                    ActionBy = h.Employee.EmployeeDetail?.FullName ?? h.Employee.Username,
                    ActionDate = h.CreatedAt
                }).ToList(),
            };
        }

        public async Task<bool> AcceptTicketAsync(TicketActionRequest request)
        {
            var ticket = await context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.Status != TicketStatus.Open)
                throw new InvalidOperationException("Ticket has already been accepted or is closed.");

            var currentUser = await context.Employees.Include(e => e.EmployeeDetail).FirstOrDefaultAsync(e => e.Id == currentUserService.UserId)
                              ?? throw new UnauthorizedAccessException();

            ticket.Status = TicketStatus.InProgress;
            ticket.Priority = request.Priority ?? ticket.Priority;
            ticket.AssignedToEmployeeId = request.AssignedToEmployeeId ?? currentUser.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            var assignedToName = request.AssignedToEmployeeId.HasValue
                ? (await context.Employees.AsNoTracking().Include(e => e.EmployeeDetail).FirstOrDefaultAsync(e => e.Id == request.AssignedToEmployeeId.Value))?.EmployeeDetail?.FullName
                : currentUser.EmployeeDetail?.FullName;

            CreateHistoryEntry(ticket.Id, currentUser.Id, "รับงาน", $"กำหนดความสำคัญเป็น: {ticket.Priority.GetDisplayName()}. มอบหมายให้: {assignedToName}");

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResolveTicketAsync(TicketActionRequest request)
        {
            var ticket = await context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
                throw new InvalidOperationException("Ticket has already been resolved or closed.");

            var currentUser = currentUserService.UserId!.Value;
            var category = await context.SupportTicketCategories.FindAsync(request.CategoryId)
                           ?? throw new KeyNotFoundException("Category not found.");

            ticket.Status = TicketStatus.Resolved;
            ticket.CategoryId = category.Id;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await AssociateFilesToTicket(ticket.Id, request.UploadedFileIds);
            CreateHistoryEntry(ticket.Id, currentUser, "ดำเนินการแก้ไขเสร็จสิ้น", $"เปลี่ยนหมวดหมู่เป็น: '{category.Name}'. บันทึก: {request.Comment}");

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var employeeId = currentUserService.UserId;
            if (employeeId == Guid.Empty)
            {
                return new List<TicketListViewModel>();
            }

            var query = context.SupportTickets
                                .AsNoTracking()
                                .Where(t => t.ReportedByEmployeeId == employeeId);

            // If no parameters are sent, default to the last 6 months.
            DateTime sDate = startDate ?? DateTime.UtcNow.AddMonths(-6);

            // If only startDate is sent, filter from that date to the present.
            // If both are sent, use the specified range.
            DateTime eDate = endDate ?? DateTime.UtcNow;

            // Ensure the end of the day is included in the filter
            eDate = eDate.Date.AddDays(1).AddTicks(-1);

            query = query.Where(t => t.CreatedAt >= sDate && t.CreatedAt <= eDate);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status,
                    // Map other necessary properties
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.SupportTickets
                                .AsNoTracking();

            // If no parameters are sent, default to the last 6 months.
            DateTime sDate = startDate ?? DateTime.UtcNow.AddMonths(-6);

            // If only startDate is sent, filter from that date to the present.
            // If both are sent, use the specified range.
            DateTime eDate = endDate ?? DateTime.UtcNow;

            // Ensure the end of the day is included in the filter
            eDate = eDate.Date.AddDays(1).AddTicks(-1);

            query = query.Where(t => t.CreatedAt >= sDate && t.CreatedAt <= eDate);

            return await query
                .Include(t => t.ReportedByEmployee).ThenInclude(e => e.EmployeeDetail)
                .Include(t => t.ReportedByEmployee).ThenInclude(e => e.Department)
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

        public async Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync(TicketCategoryType categoryType)
        {
            return await context.SupportTicketCategories
                .AsNoTracking()
                .Where(c => c.CategoryType == categoryType)
                .ToListAsync();
        }

        public async Task<SupportTicket> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request)
        {
            var userId = currentUserService.UserId!.Value;
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var descriptionBuilder = new StringBuilder("รายการเบิกอุปกรณ์:\n");
                foreach (var item in request.Items)
                {
                    var stockItem = await context.IT_Stocks.Include(s => s.Item).FirstOrDefaultAsync(s => s.ItemId == item.ItemId);
                    if (stockItem == null || stockItem.Quantity < item.Quantity)
                        throw new InvalidOperationException($"สินค้า '{stockItem?.Item.Name ?? "ID: " + item.ItemId}' มีไม่เพียงพอในสต็อก");

                    stockItem.Quantity -= item.Quantity;
                    descriptionBuilder.AppendLine($"- {stockItem.Item.Name}: {item.Quantity} {stockItem.Item.Unit}");
                }

                var category = await context.SupportTicketCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request)
                               ?? throw new InvalidOperationException("ไม่พบหมวดหมู่สำหรับการเบิก/ขออุปกรณ์");

                var ticket = new SupportTicket
                {
                    TicketNumber = await GenerateNewTicketNumberAsync(),
                    Title = "คำขอเบิกอุปกรณ์จากสต็อก",
                    Description = descriptionBuilder.ToString(),
                    CategoryId = category.Id,
                    Priority = TicketPriority.Medium,
                    Status = TicketStatus.Open,
                    RequestType = TicketRequestType.Request,
                    ReportedByEmployeeId = userId,
                    CreatedAt = DateTime.UtcNow,
                };

                context.SupportTickets.Add(ticket);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _ = emailService.SendNewTicketNotificationAsync(ticket);
                return ticket;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to create withdrawal ticket.");
                throw;
            }
        }

        public async Task<SupportTicket> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request)
        {
            var userId = currentUserService.UserId!.Value;
            var category = await context.SupportTicketCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request)
                           ?? throw new InvalidOperationException("ไม่พบหมวดหมู่สำหรับการขอจัดซื้อ");

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine($"**อุปกรณ์:** {request.ItemName}");
            descriptionBuilder.AppendLine($"**จำนวน:** {request.Quantity} ชิ้น");
            if (!string.IsNullOrWhiteSpace(request.Specification))
                descriptionBuilder.AppendLine($"**สเปค/รายละเอียด:** {request.Specification}");
            descriptionBuilder.AppendLine($"\n**เหตุผลในการขอ:**\n{request.Reason}");

            var ticket = new SupportTicket
            {
                TicketNumber = await GenerateNewTicketNumberAsync(),
                Title = $"ขอจัดซื้อ: {request.ItemName}",
                Description = descriptionBuilder.ToString(),
                CategoryId = category.Id,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open,
                RequestType = TicketRequestType.Request,
                ReportedByEmployeeId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            context.SupportTickets.Add(ticket);
            await context.SaveChangesAsync();

            _ = emailService.SendNewTicketNotificationAsync(ticket);
            return ticket;
        }

        // --- Helper Methods ---

        private void CreateHistoryEntry(int ticketId, Guid employeeId, string action, string? comment)
        {
            context.SupportTicketHistories.Add(new SupportTicketHistory
            {
                TicketId = ticketId,
                EmployeeId = employeeId,
                ActionDescription = action,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task AssociateFilesToTicket(int ticketId, List<int>? fileIds)
        {
            if (fileIds == null || !fileIds.Any()) return;

            var filesToAssociate = await context.UploadedFiles
                .Where(f => fileIds.Contains(f.Id) && f.SupportTicketId == null)
                .ToListAsync();

            if (filesToAssociate.Any())
            {
                foreach (var file in filesToAssociate)
                {
                    file.SupportTicketId = ticketId;
                }
            }
        }

        private async Task<string> GenerateNewTicketNumberAsync()
        {
            var yearMonthPrefix = DateTime.UtcNow.ToString("yyyyMM");
            var lastTicket = await context.SupportTickets
                .AsNoTracking()
                .Where(t => t.TicketNumber.StartsWith(yearMonthPrefix))
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastTicket != null && int.TryParse(lastTicket.TicketNumber.Split('-').Last(), out int lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
            return $"{yearMonthPrefix}-{nextSequence:D4}";
        }
    }
}
