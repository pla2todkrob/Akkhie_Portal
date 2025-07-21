using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Json;

namespace Portal.Models
{
    public class DepartmentRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IDepartmentRequest
    {
        public async Task<IEnumerable<DepartmentViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.DepartmentAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DepartmentViewModel>>();
            return apiResponse ?? [];
        }

        public async Task<DepartmentViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DepartmentSearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<DepartmentViewModel>();
            return apiResponse;
        }

        public async Task<IEnumerable<SectionViewModel>> GetSectionsByDepartmentIdAsync(int departmentId)
        {
            var endpoint = string.Format(_apiSettings.SectionsByDepartment, departmentId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<SectionViewModel>>();
            return apiResponse ?? [];
        }

        public async Task<ApiResponse> CreateAsync(DepartmentViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.DepartmentCreate, viewModel);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> UpdateAsync(int id, DepartmentViewModel viewModel)
        {
            var endpoint = string.Format(_apiSettings.DepartmentUpdate, id);
            var response = await _httpClient.PutAsJsonAsync(endpoint, viewModel);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DepartmentDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }
    }
}