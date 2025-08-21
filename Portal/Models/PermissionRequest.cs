using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

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

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Permission>>(_apiSettings.PermissionGetAll) ?? new List<Permission>();
        }

        public async Task<Permission> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.PermissionGetById, id);
            return await _httpClient.GetFromJsonAsync<Permission>(endpoint) ?? throw new Exception("Permission not found");
        }

        public async Task<ApiResponse<Permission>> CreateAsync(Permission model)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.PermissionCreate, model);
            return await HandleResponse<Permission>(response);
        }

        public async Task<ApiResponse> UpdateAsync(int id, Permission model)
        {
            var endpoint = string.Format(_apiSettings.PermissionUpdate, id);
            var response = await _httpClient.PutAsJsonAsync(endpoint, model);
            return await HandleApiResponse(response);
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.PermissionDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleApiResponse(response);
        }
    }
}