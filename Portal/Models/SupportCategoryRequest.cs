using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Models
{
    public class SupportCategoryRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        : BaseRequest(httpClient, apiSettings), ISupportCategoryRequest
    {
        public async Task<ApiResponse<IEnumerable<SupportCategoryViewModel>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SupportCategoryGetAll);
            return await HandleResponse<IEnumerable<SupportCategoryViewModel>>(response);
        }

        public async Task<ApiResponse<SupportCategoryViewModel>> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SupportCategoryGetById, id);
            var response = await _httpClient.GetAsync(endpoint);
            return await HandleResponse<SupportCategoryViewModel>(response);
        }

        public async Task<ApiResponse<SupportCategoryViewModel>> CreateAsync(SupportCategoryViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportCategoryCreate, model);
            return await HandleResponse<SupportCategoryViewModel>(response);
        }

        public async Task<ApiResponse<SupportCategoryViewModel>> UpdateAsync(SupportCategoryViewModel model)
        {
            var endpoint = string.Format(_apiSettings.SupportCategoryUpdate, model.Id);
            var response = await _httpClient.PutAsJsonAsync(endpoint, model);
            return await HandleResponse<SupportCategoryViewModel>(response);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SupportCategoryDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<bool>(response);
        }
    }
}
