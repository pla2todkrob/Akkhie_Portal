using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class DivisionRequest : BaseRequest, IDivisionRequest
    {
        public DivisionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<DivisionViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.DivisionAll);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DivisionViewModel>>();
            return apiResponse ?? Enumerable.Empty<DivisionViewModel>();
        }

        public async Task<DivisionViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DivisionSearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<DivisionViewModel>();
            return apiResponse ?? throw new Exception("ไม่พบข้อมูลสายงาน");
        }

        public async Task<IEnumerable<DepartmentViewModel>> GetDepartmentsByDivisionIdAsync(int divisionId)
        {
            var endpoint = string.Format(_apiSettings.DepartmentsByDivision, divisionId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<DepartmentViewModel>>();
            return apiResponse ?? Enumerable.Empty<DepartmentViewModel>();
        }
        public async Task<ApiResponse<object>> CreateAsync(DivisionViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.DivisionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, DivisionViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync(_apiSettings.DivisionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.DivisionDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}
