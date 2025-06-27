using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class DepartmentRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IDepartmentRequest
    {
        public async Task<ApiResponse<IEnumerable<DepartmentViewModel>>> AllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.DepartmentAll);
                return await HandleResponse<IEnumerable<DepartmentViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DepartmentViewModel>>.ErrorResponse($"Error fetching all divisions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<DepartmentViewModel>>> SearchByDivision(int divisionId)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.DepartmentsByDivision, divisionId);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<IEnumerable<DepartmentViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DepartmentViewModel>>.ErrorResponse($"Error fetching departments: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DepartmentViewModel>> SearchAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.DepartmentSearch, id);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<DepartmentViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DepartmentViewModel>.ErrorResponse($"Error fetching department: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DepartmentViewModel>> SaveAsync(DepartmentViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.DepartmentSave, model);
                return await HandleResponse<DepartmentViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DepartmentViewModel>.ErrorResponse($"Error saving department: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.DepartmentDelete, id);
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting department: {ex.Message}");
            }
        }
    }
}
