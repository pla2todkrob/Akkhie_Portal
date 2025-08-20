using Portal.Interfaces;
using System.Security.Claims;

namespace Portal.Models
{
    /// <summary>
    /// Service for checking user permissions within the view layer (e.g., Tag Helpers, Authorization Handlers).
    /// It caches permissions for the duration of a single HTTP request to avoid redundant API calls.
    /// </summary>
    public class ViewPermissionService : IViewPermissionService
    {
        private readonly IPermissionRequest _permissionRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserPermissionsCacheKey = "UserPermissions";

        public ViewPermissionService(IPermissionRequest permissionRequest, IHttpContextAccessor httpContextAccessor)
        {
            _permissionRequest = permissionRequest;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(string permissionKey)
        {
            var userPermissions = await GetAndCacheUserPermissionsAsync();
            if (userPermissions == null)
            {
                return false;
            }

            // The IsSystemRole claim is a quick check for superuser access without needing an API call.
            var isSystemRole = _httpContextAccessor.HttpContext?.User
                .FindFirstValue("IsSystemRole")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isSystemRole)
            {
                return true;
            }

            return userPermissions.Contains(permissionKey);
        }

        private async Task<HashSet<string>?> GetAndCacheUserPermissionsAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return new HashSet<string>();
            }

            // Check if permissions are already cached for this request
            if (context.Items.TryGetValue(UserPermissionsCacheKey, out var permissions))
            {
                return permissions as HashSet<string>;
            }

            // If not cached, fetch from the API
            var userPermissions = await _permissionRequest.GetMyPermissionsAsync();

            // Cache the permissions for the remainder of this HTTP request
            if (userPermissions != null)
            {
                context.Items[UserPermissionsCacheKey] = userPermissions;
            }

            return userPermissions;
        }
    }
}
