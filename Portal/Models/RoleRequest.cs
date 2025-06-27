using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class RoleRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IRoleRequest
    {
        public async Task<ApiResponse<IEnumerable<Role>>> AllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.RoleAll);
                return await HandleResponse<IEnumerable<Role>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<Role>>.ErrorResponse($"Error fetching all roles: {ex.Message}");
            }
        }
    }
}
