using Microsoft.Extensions.Options;
using Portal.Shared.Models.DTOs.Shared;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public abstract class BaseRequest
    {
        protected readonly HttpClient _httpClient;
        protected readonly ApiSettings _apiSettings;

        protected BaseRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        protected static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return ApiResponse<T>.ErrorResponse(
                    error?.Message ?? "Unknown error",
                    error?.Errors
                );
            }
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>()
                   ?? ApiResponse<T>.ErrorResponse("Invalid response format");
        }
    }
}
