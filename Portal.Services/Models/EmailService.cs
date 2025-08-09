using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Portal.Services.Interfaces;
using Portal.Shared.Enums;
using Portal.Shared.Models.Entities.Support;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class EmailService(
        IOptions<SmtpSettings> smtpSettings,
        PortalDbContext context,
        ILogger<EmailService> logger,
        IOptions<PortalPathUrlSettings> portalPathUrlSettings) : IEmailService
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly PortalPathUrlSettings _portalPathUrlSettings = portalPathUrlSettings.Value;
        private readonly PortalDbContext _context = context;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task SendNewTicketNotificationAsync(SupportTicket ticket)
        {
            try
            {
                var reporter = await _context.Employees
                                             .Include(e => e.EmployeeDetail)
                                             .FirstOrDefaultAsync(e => e.Id == ticket.ReportedByEmployeeId);

                if (reporter?.EmployeeDetail == null)
                {
                    _logger.LogWarning("Cannot find reporter's detail with ID {UserId} for Ticket {TicketId}", ticket.ReportedByEmployeeId, ticket.Id);
                    return;
                }

                // To: ทีม IT (Section ID = 2)
                var toEmails = await _context.Employees
                                             .Where(e => e.SectionId == 2 && e.EmployeeDetail != null && !string.IsNullOrEmpty(e.EmployeeDetail.Email))
                                             .Select(e => e.EmployeeDetail!.Email)
                                             .Distinct()
                                             .ToListAsync();

                if (toEmails.Count == 0)
                {
                    _logger.LogWarning("No recipients found in IT section (SectionId = 2) for new ticket notification.");
                    return;
                }

                // CC: ผู้แจ้งเรื่อง
                var ccEmails = new List<string> { reporter.EmployeeDetail.Email };

                var subject = $"[Portal Ticket #{ticket.TicketNumber}] {ticket.Title}";
                var body = BuildNewTicketEmailBody(ticket, reporter.EmployeeDetail.FullName);

                await SendEmailAsync(toEmails, subject, body, ccEmails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare and send new ticket notification for Ticket ID {TicketId}", ticket.Id);
            }
        }

        public async Task SendTicketResolvedNotificationAsync(SupportTicket ticket)
        {
            try
            {
                var reporter = await _context.Employees
                                             .Include(e => e.EmployeeDetail)
                                             .FirstOrDefaultAsync(e => e.Id == ticket.ReportedByEmployeeId);

                if (reporter?.EmployeeDetail?.Email == null)
                {
                    _logger.LogWarning("Cannot find reporter's email for resolved ticket {TicketId}", ticket.Id);
                    return;
                }

                var toEmail = new List<string> { reporter.EmployeeDetail.Email };
                var subject = $"[Ticket #{ticket.TicketNumber} Resolved] {ticket.Title}";
                var body = BuildResolvedEmailBody(ticket, reporter.EmployeeDetail.FullName);

                await SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation("Resolved notification sent for Ticket ID {TicketId}", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send resolved notification for Ticket ID {TicketId}", ticket.Id);
            }
        }

        public async Task SendEmailAsync(List<string> toEmails, string subject, string body, List<string>? ccEmails = null, List<string>? bccEmails = null)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            email.To.AddRange(toEmails.Select(e => MailboxAddress.Parse(e)));

            if (ccEmails != null && ccEmails.Count != 0)
                email.Cc.AddRange(ccEmails.Select(e => MailboxAddress.Parse(e)));

            if (bccEmails != null && bccEmails.Count != 0)
                email.Bcc.AddRange(bccEmails.Select(e => MailboxAddress.Parse(e)));

            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private string BuildNewTicketEmailBody(SupportTicket ticket, string reporterName)
        {
            var ticketUrl = string.Format(_portalPathUrlSettings.SupportDetails, ticket.Id);
            return $@"
                <html>
                <body style='font-family: tahoma, sans-serif; font-size: 14px;'>
                    <p>เรียน ทีมสารสนเทศ,</p>
                    <p>มีการแจ้งเรื่องใหม่เข้ามาในระบบ Portal Support</p>
                    <hr>
                    <p><strong>Ticket No:</strong> {ticket.TicketNumber}</p>
                    <p><strong>หัวข้อ:</strong> {ticket.Title}</p>
                    <p><strong>ผู้แจ้ง:</strong> {reporterName}</p>
                    <p><strong>รายละเอียด:</strong></p>
                    <div style='padding: 10px; background-color: #f5f5f5; border: 1px solid #eee; border-radius: 5px;'>
                        {ticket.Description?.Replace("\n", "<br>")}
                    </div>
                    <hr>
                    <p>กรุณาตรวจสอบและดำเนินการต่อโดยคลิกที่ลิงก์ด้านล่าง:</p>
                    <p><a href='{ticketUrl}' style='display: inline-block; padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>เปิด Ticket</a></p>
                    <br>
                    <p>ขอแสดงความนับถือ,<br>Portal System</p>
                </body>
                </html>";
        }

        private string BuildResolvedEmailBody(SupportTicket ticket, string reporterName)
        {
            var ticketUrl = string.Format(_portalPathUrlSettings.SupportDetails, ticket.Id);
            var lastHistory = _context.SupportTicketHistories
                                .Where(h => h.TicketId == ticket.Id)
                                .OrderByDescending(h => h.CreatedAt)
                                .FirstOrDefault();

            return $@"
                <html>
                <body style='font-family: tahoma, sans-serif; font-size: 14px;'>
                    <p>เรียน คุณ {reporterName},</p>
                    <p>Ticket <strong>#{ticket.TicketNumber}</strong> เรื่อง <strong>'{ticket.Title}'</strong> ของคุณได้รับการแก้ไขเรียบร้อยแล้ว</p>
                    <p><strong>รายละเอียดการแก้ไข:</strong></p>
                    <div style='padding: 10px; background-color: #f5f5f5; border: 1px solid #eee; border-radius: 5px;'>
                        {lastHistory?.Comment?.Replace("\n", "<br>")}
                    </div>
                    <hr>
                    <p>กรุณาตรวจสอบและกด 'ปิดงาน' ในระบบหากปัญหาได้รับการแก้ไขเรียบร้อยแล้ว</p>
                    <p><a href='{ticketUrl}' style='display: inline-block; padding: 10px 15px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>ไปที่ Ticket เพื่อปิดงาน</a></p>
                </body>
                </html>";
        }
    }
}
