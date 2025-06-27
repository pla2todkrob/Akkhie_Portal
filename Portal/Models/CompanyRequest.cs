using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class CompanyRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), ICompanyRequest
    {
        public async Task<ApiResponse<IEnumerable<CompanyViewModel>>> AllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.CompanyAll);
                return await HandleResponse<IEnumerable<CompanyViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CompanyViewModel>>.ErrorResponse($"Error fetching all companies: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<CompanyBranchViewModel>>> GetBranchesByCompanyAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.BranchesByCompany, id);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<IEnumerable<CompanyBranchViewModel>>(response);
            }
            catch (Exception)
            {
                return ApiResponse<IEnumerable<CompanyBranchViewModel>>.ErrorResponse("Error fetching company branches");
            }
        }

        public async Task<ApiResponse<object>> SaveAsync(CompanyViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.CompanySave, model);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return errorResponse ?? ApiResponse.ErrorResponse("เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ");
                }

                var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return successResponse ?? ApiResponse<object>.ErrorResponse("Unexpected null response from the server");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResponse($"Error saving company: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CompanyViewModel>> SearchAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.CompanySearch, id);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<CompanyViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyViewModel>.ErrorResponse($"Error searching company: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.CompanyDelete, id);
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting company: {ex.Message}");
            }
        }
    }
}
