using System.Net.Http.Headers;
using System.Security.Claims;

namespace Portal.Models
{
    /// <summary>
    /// A DelegatingHandler that automatically attaches the JWT Bearer token
    /// from the user's claims to every outgoing HttpClient request.
    /// </summary>
    public class AuthenticationHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Try to get the access token from the current user's claims.
            var accessToken = httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                // If the token exists, add it to the Authorization header.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Continue with the request pipeline.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
