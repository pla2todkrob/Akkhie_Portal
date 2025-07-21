using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.Entities;

namespace Portal.Models
{
    public class RoleRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IRoleRequest
    {
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.RoleAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Role>>();
            return apiResponse ?? [];
        }
    }
}