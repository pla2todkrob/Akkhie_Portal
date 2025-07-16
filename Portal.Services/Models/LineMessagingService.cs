using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities.Support;
using System.Net.Http.Headers;
using System.Text;

namespace Portal.Services.Models
{
    public class LineMessagingService : ILineMessagingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _channelAccessToken;
        private readonly string _supportGroupId;

        public LineMessagingService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _channelAccessToken = configuration["LineMessagingApiSettings:ChannelAccessToken"];
            _supportGroupId = configuration["LineMessagingApiSettings:GroupId"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
        }

        public async Task SendPushMessageAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(_channelAccessToken) || string.IsNullOrEmpty(to)) return;

            var payload = new
            {
                to,
                messages = new[]
                {
                    new { type = "text", text = message }
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://api.line.me/v2/bot/message/push", payload);
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                // Log error if needed, but don't crash the application
            }
        }

        public async Task SendTicketCreationNotificationAsync(SupportTicket ticket)
        {
            if (string.IsNullOrEmpty(_supportGroupId)) return;

            var sb = new StringBuilder();
            sb.AppendLine("🔔 มี Ticket ใหม่เข้ามาในระบบ!");
            sb.AppendLine($"หมายเลข: {ticket.TicketNumber}");
            sb.AppendLine($"หัวข้อ: {ticket.Title}");
            sb.AppendLine($"ผู้แจ้ง: {ticket.ReportedByEmployee.EmployeeDetail.LocalFullName}");
            sb.AppendLine($"แผนก: {ticket.ReportedByEmployee.Section.Name}");
            sb.AppendLine($"วันที่: {ticket.CreatedAt:dd/MM/yyyy HH:mm}");

            await SendPushMessageAsync(_supportGroupId, sb.ToString());
        }
    }
}
