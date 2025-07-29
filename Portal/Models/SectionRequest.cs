using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Models
{
    public class SectionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), ISectionRequest
    {
        public async Task<IEnumerable<SectionViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SectionAll);
            var apiResponse = await HandleResponse<IEnumerable<SectionViewModel>>(response);
            return apiResponse.Data ?? [];
        }

        public async Task<SectionViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SectionSearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await HandleResponse<SectionViewModel>(response);
            return apiResponse.Data;
        }

        public async Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId)
        {
            var endpoint = string.Format(_apiSettings.SectionsByDepartment, departmentId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await HandleResponse<IEnumerable<SectionViewModel>>(response);
            return apiResponse.Data ?? [];
        }

        public async Task<ApiResponse<object>> CreateAsync(SectionViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SectionCreate, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, SectionViewModel viewModel)
        {
            var endpoint = string.Format(_apiSettings.SectionUpdate, id);
            var response = await _httpClient.PutAsJsonAsync(endpoint, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SectionDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}