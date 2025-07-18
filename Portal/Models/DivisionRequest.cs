using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class DivisionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IDivisionRequest
    {
        // Use the 'new' keyword to explicitly hide the inherited member
        private readonly new HttpClient _httpClient = httpClient;

        public async Task<IEnumerable<DivisionViewModel>> GetAll()
        {
            var response = await _httpClient.GetAsync("api/division");
            var apiResponse = await HandleResponse<IEnumerable<DivisionViewModel>>(response);
            return apiResponse.Success && apiResponse.Data != null ? apiResponse.Data : [];
        }

        public async Task<DivisionViewModel> GetById(int id)
        {
            var response = await _httpClient.GetAsync($"api/division/{id}");
            var apiResponse = await HandleResponse<DivisionViewModel>(response);
            return apiResponse.Success && apiResponse.Data != null ? apiResponse.Data : throw new InvalidOperationException("Division not found.");
        }

        public async Task<ApiResponse> Create(DivisionViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/division", viewModel);
            var apiResponse = await HandleResponse<object>(response);
            return new ApiResponse
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message,
                Data = apiResponse.Data,
                Errors = apiResponse.Errors
            };
        }

        public async Task<ApiResponse> Update(int id, DivisionViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/division/{id}", viewModel);
            var apiResponse = await HandleResponse<object>(response);
            return new ApiResponse
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message,
                Data = apiResponse.Data,
                Errors = apiResponse.Errors
            };
        }

        public async Task<ApiResponse> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/division/{id}");
            var apiResponse = await HandleResponse<object>(response);
            return new ApiResponse
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message,
                Data = apiResponse.Data,
                Errors = apiResponse.Errors
            };
        }

        public async Task<IEnumerable<SelectListItem>> GetLookupByCompany(int companyId)
        {
            var response = await _httpClient.GetAsync($"api/lookup/divisions/company/{companyId}");
            var apiResponse = await HandleResponse<IEnumerable<SelectListItem>>(response);
            return apiResponse.Success && apiResponse.Data != null ? apiResponse.Data : [];
        }
    }
}
