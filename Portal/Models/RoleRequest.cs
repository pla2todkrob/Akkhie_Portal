using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class RoleRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IRoleRequest
    {

        public async Task<ApiResponse<IEnumerable<Role>>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.RoleAll);
                return await HandleResponse<IEnumerable<Role>>(response);
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse<IEnumerable<Role>>.ErrorResponse($"Error fetching all roles: {ex.Message}");
            }
        }
    }
}
