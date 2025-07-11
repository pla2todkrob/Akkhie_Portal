using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class EmployeeRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IEmployeeRequest
    {
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    _apiSettings.EmployeeLogin,
                    request
                );
                return await HandleResponse<LoginResponse>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.ErrorResponse($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    _apiSettings.EmployeeRegister,
                    request
                );
                return await HandleResponse<RegisterResponse>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<RegisterResponse>.ErrorResponse($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<EmployeeViewModel>> SearchAsync(Guid employeeId)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.EmployeeSearch, employeeId);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<EmployeeViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EmployeeViewModel>.ErrorResponse($"Error fetching employee: {ex.Message}");
            }
        }

        public async Task<ApiResponse<EmployeeViewModel>> SearchByUsernameAsync(string username)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.EmployeeSearchByUsername, username);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<EmployeeViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EmployeeViewModel>.ErrorResponse($"Error fetching employee by username: {ex.Message}");
            }
        }

        public async Task<ApiResponse<EmployeeViewModel>> SearchByEmailAsync(string email)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.EmployeeSearchByEmail, email);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<EmployeeViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EmployeeViewModel>.ErrorResponse($"Error fetching employee by email: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<EmployeeViewModel>>> GetAsync(int? companyId = null, int? divisionId = null, int? departmentId = null, int? sectionId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (companyId.HasValue) queryParams.Add($"companyId={companyId.Value}");
                if (divisionId.HasValue) queryParams.Add($"divisionId={divisionId.Value}");
                if (departmentId.HasValue) queryParams.Add($"departmentId={departmentId.Value}");
                if (sectionId.HasValue) queryParams.Add($"sectionId={sectionId.Value}");

                var endpoint = _apiSettings.EmployeeAll;
                if (queryParams.Count != 0)
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<IEnumerable<EmployeeViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<EmployeeViewModel>>.ErrorResponse($"Error fetching employees: {ex.Message}");
            }
        }

    }
}
