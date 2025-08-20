using Microsoft.Extensions.Options;
using Portal.Interfaces;

namespace Portal.Models
{
    public class PermissionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        : BaseRequest(httpClient, apiSettings), IPermissionRequest
    {
        public async Task<HashSet<string>?> GetMyPermissionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.PermissionGetUserPermissions);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HashSet<string>>();
                }
                return null;
            }
            catch (Exception)
            {
                // Log error in a real application
                return null;
            }
        }
    }
}