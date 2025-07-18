using Microsoft.Extensions.Options;
using Portal.Shared.Models.DTOs.Shared;
using System.Net.Http.Headers;
using System.Text.Json; // เพิ่มเข้ามาเพื่อใช้ JsonException

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

        // [IMPROVEMENT] เพิ่ม try-catch เพื่อให้การจัดการ Response ทนทานต่อข้อผิดพลาดมากขึ้น
        protected static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    // สำหรับ errors, body อาจจะเป็น ApiResponse แบบไม่มี generic
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return ApiResponse<T>.ErrorResponse(
                        errorResponse?.Message ?? $"HTTP Error: {(int)response.StatusCode} {response.ReasonPhrase}",
                        errorResponse?.Errors
                    );
                }

                // สำหรับ success, body ควรจะเป็น ApiResponse<T>
                var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                return successResponse ?? ApiResponse<T>.ErrorResponse("API returned a null response.");
            }
            catch (JsonException ex)
            {
                // ดักจับ Error กรณีที่ body ของ response ไม่ใช่ JSON ที่ถูกต้อง
                return ApiResponse<T>.ErrorResponse($"Failed to parse API response: {ex.Message}");
            }
            catch (Exception ex)
            {
                // ดักจับ Error อื่นๆ ที่ไม่คาดคิด
                return ApiResponse<T>.ErrorResponse($"An unexpected error occurred while handling the API response: {ex.Message}");
            }
        }
    }
}
