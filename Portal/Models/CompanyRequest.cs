using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Json;

namespace Portal.Models
{
    public class CompanyRequest : BaseRequest, ICompanyRequest
    {
        public CompanyRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<CompanyViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.CompanyAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<CompanyViewModel>>();
            return apiResponse ?? Enumerable.Empty<CompanyViewModel>();
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
            return apiResponse ?? Enumerable.Empty<CompanyBranchViewModel>();
        }

        public async Task<IEnumerable<DivisionViewModel>> GetDivisionsByCompanyIdAsync(int companyId)
        {
            var endpoint = string.Format(_apiSettings.DivisionsByCompany, companyId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DivisionViewModel>>();
            return apiResponse ?? Enumerable.Empty<DivisionViewModel>();
        }

        public async Task<ApiResponse<object>> CreateAsync(CompanyViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.CompanySave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, CompanyViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync(_apiSettings.CompanySave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.CompanyDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}