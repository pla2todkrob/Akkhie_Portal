using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class DivisionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IDivisionRequest
    {
        public async Task<ApiResponse<IEnumerable<DivisionViewModel>>> AllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.DivisionAll);
                return await HandleResponse<IEnumerable<DivisionViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DivisionViewModel>>.ErrorResponse($"Error fetching all divisions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DivisionViewModel>> SearchAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.DivisionSearch, id);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<DivisionViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DivisionViewModel>.ErrorResponse($"Error fetching division with ID {id}: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> SaveAsync(DivisionViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.DivisionSave, model);
                return await HandleResponse<object>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Error saving division: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.DivisionDelete, id);
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting division with ID {id}: {ex.Message}");
            }
        }
    }
}
