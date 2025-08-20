using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;
using System.Text;

namespace Portal.Services.Models
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly PortalDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly ILogger<SupportTicketService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupportTicketService(
            PortalDbContext context,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IFileService fileService,
            ILogger<SupportTicketService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _fileService = fileService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TicketDetailViewModel?> GetTicketDetailsAsync(int ticketId)
        {
            var ticket = await _context.SupportTickets
               .AsNoTracking()
               .Where(t => t.Id == ticketId)
               .Include(t => t.Category)
               .Include(t => t.ReportedByEmployee).ThenInclude(e => e.EmployeeDetail)
               .Include(t => t.AssignedToEmployee).ThenInclude(e => e!.EmployeeDetail)
               .Include(t => t.History).ThenInclude(h => h.Employee).ThenInclude(e => e.EmployeeDetail)
               .Include(t => t.SupportTicketFiles).ThenInclude(f => f.UploadedFile)
               .Include(t => t.RelatedTicket)
               .FirstOrDefaultAsync();

            if (ticket == null) return null;

            int queuePosition = 0;
            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.InProgress)
            {
                queuePosition = await _context.SupportTickets
                    .AsNoTracking()
                    .CountAsync(t =>
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
                        t.CreatedAt < ticket.CreatedAt
                    ) + 1;
            }

            var request = _httpContextAccessor.HttpContext!.Request;
            var apiBaseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            var attachments = ticket.SupportTicketFiles.Select(fileRelation =>
            {
                var uploadedFile = fileRelation.UploadedFile;
                return new TicketDetailViewModel.AttachmentItem
                {
                    FileName = uploadedFile.OriginalFileName,
                    FileSizeDisplay = uploadedFile.FileSizeDisplay,
                    FileUrl = $"{apiBaseUrl}/uploads/{uploadedFile.UploadPath.Replace('\\', '/')}"
                };
            }).ToList();

            var currentUserId = _currentUserService.UserId;
            var isOwner = currentUserId.HasValue && ticket.ReportedByEmployeeId == currentUserId.Value;

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
                History = ticket.History.OrderBy(h => h.CreatedAt).Select(h => new TicketDetailViewModel.HistoryItem
                {
                    ActionDescription = h.ActionDescription,
                    Comment = h.Comment,
                    ActionBy = h.Employee.EmployeeDetail?.FullName ?? h.Employee.Username,
                    ActionDate = h.CreatedAt
                }).ToList(),
                Attachments = attachments,
                ReportedById = ticket.ReportedByEmployeeId,
                IsOwner = isOwner,
                QueuePosition = queuePosition,
            };
        }

        public async Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = _currentUserService.UserId!.Value;
                var defaultCategory = await _context.SupportTicketCategories
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Issue && c.IsNotCategory)
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

                _context.SupportTickets.Add(newTicket);
                await _context.SaveChangesAsync();

                if (request.UploadedFiles != null && request.UploadedFiles.Any())
                {
                    var uploadedFileEntities = await _fileService.UploadFilesAsync(request.UploadedFiles);
                    foreach (var fileEntity in uploadedFileEntities)
                    {
                        var supportTicketFile = new SupportTicketFiles
                        {
                            SupportTicketId = newTicket.Id,
                            UploadedFileId = fileEntity.Id
                        };
                        _context.SupportTicketFiles.Add(supportTicketFile);
                    }
                }

                CreateHistoryEntry(newTicket.Id, userId, "สร้าง Ticket ใหม่", "ระบบสร้าง Ticket อัตโนมัติ");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var queuePosition = await _context.SupportTickets
                    .AsNoTracking()
                    .CountAsync(t =>
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
                        t.CreatedAt < newTicket.CreatedAt
                    ) + 1;

                await _emailService.SendNewTicketNotificationAsync(newTicket, queuePosition);

                _logger.LogInformation("Successfully created Ticket #{TicketNumber} by user {UserId}", newTicket.TicketNumber, userId);

                return newTicket;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create ticket with attachments. Transaction rolled back.");
                throw;
            }
        }

        public async Task<bool> AcceptTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("ไม่พบ Ticket");

            if (ticket.Status != TicketStatus.Open)
                throw new InvalidOperationException("Ticket นี้ถูกรับงานไปแล้วหรือถูกปิดไปแล้ว");

            var currentUserId = _currentUserService.UserId!.Value;

            ticket.Status = TicketStatus.InProgress;
            ticket.Priority = request.Priority ?? ticket.Priority;
            ticket.AssignedToEmployeeId = request.AssignedToEmployeeId ?? currentUserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            var assignedTo = await _context.Employees.Include(e => e.EmployeeDetail)
                                .FirstOrDefaultAsync(e => e.Id == ticket.AssignedToEmployeeId);

            CreateHistoryEntry(ticket.Id, currentUserId, "รับงาน", $"กำหนดความสำคัญเป็น: {ticket.Priority.GetDisplayName()}. มอบหมายให้: {assignedTo?.EmployeeDetail?.FullName}");

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResolveTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("ไม่พบ Ticket");

            if (ticket.Status != TicketStatus.InProgress)
                throw new InvalidOperationException("Ticket ต้องอยู่ในสถานะ InProgress เท่านั้น");

            var currentUserId = _currentUserService.UserId!.Value;
            var category = await _context.SupportTicketCategories.FindAsync(request.CategoryId)
                           ?? throw new KeyNotFoundException("ไม่พบหมวดหมู่");

            ticket.Status = TicketStatus.Resolved;
            ticket.CategoryId = category.Id;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            CreateHistoryEntry(ticket.Id, currentUserId, "ดำเนินการแก้ไขเสร็จสิ้น", $"เปลี่ยนหมวดหมู่เป็น: '{category.Name}'.\nหมายเหตุ: {request.Comment}");

            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                await _emailService.SendTicketResolvedNotificationAsync(ticket);
            }
            return result;
        }

        public async Task<bool> CloseTicketByUserAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("ไม่พบ Ticket");

            var currentUserId = _currentUserService.UserId!.Value;

            if (ticket.ReportedByEmployeeId != currentUserId)
                throw new UnauthorizedAccessException("คุณไม่ใช่เจ้าของ Ticket นี้");

            if (ticket.Status != TicketStatus.Resolved)
                throw new InvalidOperationException("Ticket ยังไม่ได้รับการแก้ไข");

            ticket.Status = TicketStatus.Closed;
            ticket.UpdatedAt = DateTime.UtcNow;

            CreateHistoryEntry(ticket.Id, currentUserId, "ผู้ใช้ปิดงาน", request.Comment ?? "ผู้ใช้ยืนยันการแก้ไขและปิดงาน");

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("ไม่พบ Ticket");

            var currentUserId = _currentUserService.UserId!.Value;

            if (ticket.ReportedByEmployeeId != currentUserId)
                throw new UnauthorizedAccessException("คุณไม่ใช่เจ้าของ Ticket นี้");

            if (ticket.Status != TicketStatus.Open && ticket.Status != TicketStatus.InProgress)
                throw new InvalidOperationException("ไม่สามารถยกเลิก Ticket ที่ถูกแก้ไขหรือปิดไปแล้วได้");

            ticket.Status = TicketStatus.Cancelled;
            ticket.UpdatedAt = DateTime.UtcNow;

            CreateHistoryEntry(ticket.Id, currentUserId, "ยกเลิก Ticket", $"เหตุผล: {request.Comment}");

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectTicketAsync(TicketActionRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(request.TicketId)
                         ?? throw new KeyNotFoundException("ไม่พบ Ticket");

            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
                throw new InvalidOperationException("ไม่สามารถปฏิเสธ Ticket ที่มีสถานะ Resolved, Closed, หรือ Cancelled ได้");

            if (string.IsNullOrWhiteSpace(request.Comment))
                throw new InvalidOperationException("กรุณาระบุเหตุผลในการปฏิเสธ");

            var currentUserId = _currentUserService.UserId!.Value;

            ticket.Status = TicketStatus.Rejected;
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.AssignedToEmployeeId = ticket.AssignedToEmployeeId ?? currentUserId;

            CreateHistoryEntry(ticket.Id, currentUserId, "ปฏิเสธ Ticket", $"เหตุผล: {request.Comment}");

            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                _logger.LogInformation("Ticket #{TicketNumber} was rejected by user {UserId}", ticket.TicketNumber, currentUserId);
            }
            return result;
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var employeeId = _currentUserService.UserId;
            if (employeeId == Guid.Empty)
            {
                return new List<TicketListViewModel>();
            }

            var query = _context.SupportTickets
                                .AsNoTracking()
                                .Where(t => t.ReportedByEmployeeId == employeeId);

            DateTime sDate = startDate ?? DateTime.UtcNow.AddMonths(-6);
            DateTime eDate = endDate ?? DateTime.UtcNow;
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
                    StatusName = t.Status.GetDisplayName(),
                    CategoryName = t.Category.Name,
                    DepartmentName = t.ReportedByEmployee.Department != null ? t.ReportedByEmployee.Department.Name : "N/A",
                    ReportedBy = t.ReportedByEmployee.EmployeeDetail != null ? t.ReportedByEmployee.EmployeeDetail.FullName : t.ReportedByEmployee.Username,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyClosedTicketsAsync()
        {
            var employeeId = _currentUserService.UserId;
            if (!employeeId.HasValue)
            {
                return Enumerable.Empty<TicketListViewModel>();
            }

            return await _context.SupportTickets
                .AsNoTracking()
                .Where(t => t.ReportedByEmployeeId == employeeId.Value && t.Status == TicketStatus.Closed)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SupportTickets
                                .AsNoTracking();

            DateTime sDate = startDate ?? DateTime.UtcNow.AddMonths(-6);
            DateTime eDate = endDate ?? DateTime.UtcNow;
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
            return await _context.SupportTicketCategories
                .AsNoTracking()
                .Where(c => c.CategoryType == categoryType)
                .ToListAsync();
        }

        public async Task<SupportTicket> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request)
        {
            var userId = _currentUserService.UserId!.Value;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var descriptionBuilder = new StringBuilder("รายการเบิกอุปกรณ์:\n");
                foreach (var item in request.Items)
                {
                    var stockItem = await _context.IT_Stocks.Include(s => s.Item).FirstOrDefaultAsync(s => s.ItemId == item.ItemId);
                    if (stockItem == null || stockItem.Quantity < item.Quantity)
                        throw new InvalidOperationException($"สินค้า '{stockItem?.Item.Name ?? "ID: " + item.ItemId}' มีไม่เพียงพอในสต็อก");

                    stockItem.Quantity -= item.Quantity;
                    descriptionBuilder.AppendLine($"- {stockItem.Item.Name}: {item.Quantity} {stockItem.Item.Unit}");
                }

                var category = await _context.SupportTicketCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request)
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

                _context.SupportTickets.Add(ticket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var queuePosition = await _context.SupportTickets
                    .AsNoTracking()
                    .CountAsync(t => (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) && t.CreatedAt < ticket.CreatedAt) + 1;

                await _emailService.SendNewTicketNotificationAsync(ticket, queuePosition);
                return ticket;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create withdrawal ticket.");
                throw;
            }
        }

        public async Task<SupportTicket> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request)
        {
            var userId = _currentUserService.UserId!.Value;
            var category = await _context.SupportTicketCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryType == TicketCategoryType.Request)
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

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var queuePosition = await _context.SupportTickets
                .AsNoTracking()
                .CountAsync(t => (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) && t.CreatedAt < ticket.CreatedAt) + 1;
            await _emailService.SendNewTicketNotificationAsync(ticket, queuePosition);
            return ticket;
        }

        private void CreateHistoryEntry(int ticketId, Guid employeeId, string action, string? comment)
        {
            _context.SupportTicketHistories.Add(new SupportTicketHistory
            {
                TicketId = ticketId,
                EmployeeId = employeeId,
                ActionDescription = action,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task<string> GenerateNewTicketNumberAsync()
        {
            var yearMonthPrefix = DateTime.UtcNow.ToString("yyyyMM");
            var lastTicket = await _context.SupportTickets
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
