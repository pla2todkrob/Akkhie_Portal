using Portal.Shared.Enums;

namespace Portal.Shared.Models.DTOs.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public Guid? EmployeeId { get; set; }
        public EmployeeStatus EmployeeStatus { get; set; }
        public List<string>? Errors { get; set; }
        public string? Token { get; set; }
    }
}
