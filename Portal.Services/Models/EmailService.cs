using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Portal.Services.Interfaces;
using Portal.Shared.Enums;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.Entities.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class EmailService(IOptions<SmtpSettings> smtpSettings, PortalDbContext context, ILogger<EmailService> logger, IOptions<PortalPathUrlSettings> portalPathUrlSettings) : IEmailService
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly PortalPathUrlSettings _portalPathUrlSettings = portalPathUrlSettings.Value;

        public async Task SendNewTicketNotificationAsync(SupportTicket ticket)
        {
            try
            {
                // 1. ดึงข้อมูลผู้ที่เกี่ยวข้อง (Reporter) โดยดึงข้อมูล Detail และ Role มาด้วย
                var reporter = await context.Employees
                                             .Include(e => e.EmployeeDetail)
                                             .FirstOrDefaultAsync(e => e.Id == ticket.ReportedByEmployeeId);

                if (reporter?.EmployeeDetail == null)
                {
                    logger.LogWarning("Cannot find reporter's detail with ID {UserId} for Ticket {TicketId}", ticket.ReportedByEmployeeId, ticket.Id);
                    return;
                }

                // 2. ค้นหาผู้รับอีเมลตามเงื่อนไข
                // To: ทีม IT (Section ID = 2)
                var toEmails = new List<string>();
                var supportTeams = await context.Employees
                                             .Include(e => e.EmployeeDetail)
                                             .Where(e => e.SectionId == 2)
                                             .ToListAsync();
                if (supportTeams.Any())
                {
                    toEmails = [.. supportTeams.Where(e => e.EmployeeDetail != null && !string.IsNullOrEmpty(e.EmployeeDetail.Email))
                                           .Select(e => e.EmployeeDetail.Email)
                                           .Distinct()];
                }


                if (!toEmails.Any())
                {
                    logger.LogWarning("No recipients found in IT section (SectionId = 2) for new ticket notification.");
                    return;
                }

                // CC: ผู้แจ้งเรื่อง
                var ccEmail = reporter.EmployeeDetail.Email;

                // BCC: ผู้จัดการ (Role ID = 5) - ยกเว้นกรณีผู้แจ้งเป็นผู้จัดการอยู่แล้ว
                var bccEmails = new List<string>();
                if (reporter.RoleId != (int)RoleType.DepartmentManager) // ใช้ Enum เพื่อความชัดเจน
                {
                    bccEmails = await context.Employees
                                              .Include(e => e.EmployeeDetail) // <-- Include EmployeeDetail
                                              .Where(e => e.RoleId == (int)RoleType.DepartmentManager && e.EmployeeDetail != null && !string.IsNullOrEmpty(e.EmployeeDetail.Email))
                                              .Select(e => e.EmployeeDetail.Email)
                                              .Distinct()
                                              .ToListAsync();
                }

                // 3. สร้างอีเมล
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                email.To.AddRange(toEmails.Select(e => MailboxAddress.Parse(e)));

                if (!string.IsNullOrEmpty(ccEmail))
                {
                    email.Cc.Add(MailboxAddress.Parse(ccEmail));
                }

                if (bccEmails.Any())
                {
                    email.Bcc.AddRange(bccEmails.Select(e => MailboxAddress.Parse(e)));
                }

                email.Subject = $"[Portal Ticket #{ticket.TicketNumber}] {ticket.Title}";

                // ใช้ FullName จาก EmployeeDetail
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = BuildEmailBody(ticket, reporter.EmployeeDetail.FullName)
                };

                // 4. ส่งอีเมลผ่าน SMTP Server
                using var smtp = new SmtpClient();
                // IMAP is for receiving mail, SMTP is for sending. We use SMTP here.
                await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                logger.LogInformation("New ticket notification sent successfully for Ticket ID {TicketId}", ticket.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send new ticket notification for Ticket ID {TicketId}", ticket.Id);
            }
        }

        private string BuildEmailBody(SupportTicket ticket, string reporterName)
        {
            var ticketUrl = string.Format(_portalPathUrlSettings.SupportDetails, ticket.Id);
            var body = $@"
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
                        {ticket.Description.Replace("\n", "<br>")}
                    </div>
                    <hr>
                    <p>กรุณาตรวจสอบและดำเนินการต่อโดยคลิกที่ลิงก์ด้านล่าง:</p>
                    <p><a href='{ticketUrl}' style='display: inline-block; padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>เปิด Ticket</a></p>
                    <br>
                    <p>ขอแสดงความนับถือ,<br>Portal System</p>
                </body>
                </html>";
            return body;
        }
    }
}
