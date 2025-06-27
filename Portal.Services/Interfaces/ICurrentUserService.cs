// Services/Interfaces/ICurrentUserService.cs
namespace Portal.Services.Models
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Username { get; }
        string? IpAddress { get; }
        bool IsAuthenticated { get; }
    }
}
