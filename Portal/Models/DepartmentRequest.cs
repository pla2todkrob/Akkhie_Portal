using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Json;

namespace Portal.Models
{
    public class DepartmentRequest : BaseRequest, IDepartmentRequest
    {
        public DepartmentRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<DepartmentViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.DepartmentAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DepartmentViewModel>>();
            return apiResponse ?? Enumerable.Empty<DepartmentViewModel>();
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
            return apiResponse ?? Enumerable.Empty<SectionViewModel>();
        }

        public async Task<ApiResponse<object>> CreateAsync(DepartmentViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.DepartmentSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, DepartmentViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync(_apiSettings.DepartmentSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DepartmentDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}