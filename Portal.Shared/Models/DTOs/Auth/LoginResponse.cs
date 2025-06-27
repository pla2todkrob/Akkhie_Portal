namespace Portal.Shared.Models.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string>? ActiveDirectoryProperties { get; set; }
        public bool IsNewUser { get; set; }
        public string? Username { get; set; }
    }
}
