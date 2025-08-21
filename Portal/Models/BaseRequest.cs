using Microsoft.Extensions.Options;
using Portal.Shared.Models.DTOs.Shared;
using System.Net.Http.Headers;
using System.Text.Json;

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
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return ApiResponse<T>.ErrorResponse(
                        errorResponse?.Message ?? $"HTTP Error: {(int)response.StatusCode} {response.ReasonPhrase}",
                        errorResponse?.Errors
                    );
                }

                var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                return successResponse ?? ApiResponse<T>.ErrorResponse("API returned a null response.");
            }
            catch (JsonException ex)
            {
                return ApiResponse<T>.ErrorResponse($"Failed to parse API response: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse($"An unexpected error occurred while handling the API response: {ex.Message}");
            }
        }

        protected static async Task<ApiResponse> HandleApiResponse(HttpResponseMessage response)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return response.IsSuccessStatusCode
                        ? ApiResponse.SuccessResponse("Operation completed successfully.")
                        : ApiResponse.ErrorResponse($"HTTP Error: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse ?? ApiResponse.ErrorResponse("Failed to deserialize API response.");
            }
            catch (JsonException ex)
            {
                return ApiResponse.ErrorResponse($"Failed to parse API response: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResponse($"An unexpected error occurred while handling the API response: {ex.Message}");
            }
        }
    }
}
