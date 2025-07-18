using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Models
{
    public class DivisionRequest : BaseRequest, IDivisionRequest
    {
        public DivisionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<DivisionViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.DivisionAll);
            var apiResponse = await HandleResponse<IEnumerable<DivisionViewModel>>(response);
            return apiResponse.Data ?? Enumerable.Empty<DivisionViewModel>();
        }

        public async Task<DivisionViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DivisionSearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await HandleResponse<DivisionViewModel>(response);
            return apiResponse.Data;
        }

        // [FIX] เปลี่ยน return type และการเรียก HandleResponse
        public async Task<ApiResponse<object>> CreateAsync(DivisionViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.DivisionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        // [FIX] เปลี่ยน return type และการเรียก HandleResponse
        public async Task<ApiResponse<object>> UpdateAsync(int id, DivisionViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync(_apiSettings.DivisionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        // [FIX] เปลี่ยน return type และการเรียก HandleResponse
        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DivisionDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}