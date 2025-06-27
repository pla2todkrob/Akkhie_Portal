using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.DTOs.Shared;

namespace Portal.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);

        Task<RegisterResponse> RegisterAsync(RegisterRequest request, string? ipAddress);
    }
}
