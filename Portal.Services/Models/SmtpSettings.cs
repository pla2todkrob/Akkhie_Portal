﻿namespace Portal.Services.Models
{
    /// <summary>
    /// Class สำหรับการตั้งค่า SMTP Server
    /// </summary>
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TicketUrlTemplate { get; set; }
    }
}
