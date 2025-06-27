using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class SectionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), ISectionRequest
    {
        public async Task<ApiResponse<IEnumerable<SectionViewModel>>> AllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.SectionAll);
                return await HandleResponse<IEnumerable<SectionViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SectionViewModel>>.ErrorResponse($"Error fetching all sections: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SectionViewModel>> SearchAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.SectionSearch, id);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<SectionViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SectionViewModel>.ErrorResponse($"Error fetching section: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<SectionViewModel>>> SearchByDepartment(int departmentId)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.SectionsByDepartment, departmentId);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<IEnumerable<SectionViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SectionViewModel>>.ErrorResponse($"Error fetching sections: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SectionViewModel>> SaveAsync(SectionViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.SectionSave, model);
                return await HandleResponse<SectionViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SectionViewModel>.ErrorResponse($"Error saving section: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.SectionDelete, id);
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting section: {ex.Message}");
            }
        }
    }
}
