namespace Portal.Services.Models
{
    public class ActiveDirectorySettings
    {
        public static string SectionName = "ActiveDirectory";
        public string Server { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string BindUser { get; set; } = string.Empty;
        public string BindPassword { get; set; } = string.Empty;
        public bool UseSecureConnection { get; set; }
    }
}
