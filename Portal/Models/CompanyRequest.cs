using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Json;

namespace Portal.Models
{
    public class CompanyRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), ICompanyRequest
    {
        public async Task<IEnumerable<CompanyViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.CompanyAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<CompanyViewModel>>();
            return apiResponse ?? [];
        }

        public async Task<CompanyViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.CompanySearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<CompanyViewModel>();
            return apiResponse ?? throw new Exception("ไม่พบข้อมูลบริษัท");
        }

        public async Task<IEnumerable<CompanyBranchViewModel>> GetBranchesByCompanyIdAsync(int companyId)
        {
            var endpoint = string.Format(_apiSettings.BranchesByCompany, companyId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<CompanyBranchViewModel>>();
            return apiResponse ?? [];
        }

        public async Task<IEnumerable<DivisionViewModel>> GetDivisionsByCompanyIdAsync(int companyId)
        {
            var endpoint = string.Format(_apiSettings.DivisionsByCompany, companyId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DivisionViewModel>>();
            return apiResponse ?? [];
        }

        public async Task<ApiResponse> CreateAsync(CompanyViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.CompanyCreate, viewModel);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> UpdateAsync(int id, CompanyViewModel viewModel)
        {
            var endpoint = string.Format(_apiSettings.CompanyUpdate, id);
            var response = await _httpClient.PutAsJsonAsync(endpoint, viewModel);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.CompanyDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }
    }
}