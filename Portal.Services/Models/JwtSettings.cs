namespace Portal.Services.Models
{
    public class JwtSettings
    {
        public static string SectionName = "Jwt";
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
