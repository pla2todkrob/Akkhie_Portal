using System.Security.Claims;

namespace Portal.Services.Models
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public Guid? UserId
        {
            get
            {
                var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public string? Username =>
            httpContextAccessor.HttpContext?.User.Identity?.Name;

        public string? IpAddress =>
            httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        public bool IsAuthenticated =>
            httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}
