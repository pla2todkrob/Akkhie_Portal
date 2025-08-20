using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;
using System.Net.Http.Headers;

namespace Portal.Models
{
    public class SupportTicketRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        : BaseRequest(httpClient, apiSettings), ISupportTicketRequest
    {
        public async Task<ApiResponse<SupportTicket>> CreateTicketAsync(CreateTicketRequest model)
        {
            try
            {
                using var multipartContent = new MultipartFormDataContent
                {
                    { new StringContent(model.Title ?? string.Empty), nameof(model.Title) },
                    { new StringContent(model.Description ?? string.Empty), nameof(model.Description) }
                };
                if (model.RelatedTicketId.HasValue)
                {
                    multipartContent.Add(new StringContent(model.RelatedTicketId.Value.ToString()), nameof(model.RelatedTicketId));
                }

                if (model.UploadedFiles != null)
                {
                    foreach (var file in model.UploadedFiles)
                    {
                        if (file.Length > 0)
                        {
                            var streamContent = new StreamContent(file.OpenReadStream());
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                            multipartContent.Add(streamContent, nameof(model.UploadedFiles), file.FileName);
                        }
                    }
                }

                var response = await _httpClient.PostAsync(_apiSettings.SupportTicketCreate, multipartContent);

                return await HandleResponse<SupportTicket>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupportTicket>.ErrorResponse($"An error occurred in SupportTicketRequest: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync(string categoryType)
        {
            var endpoint = $"{_apiSettings.SupportTicketGetCategories}?categoryType={categoryType}";
            var response = await _httpClient.GetAsync(endpoint);
            return await response.Content.ReadFromJsonAsync<IEnumerable<SupportTicketCategory>>()
                   ?? throw new Exception("Failed to deserialize response");
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyTicketsAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SupportTicketGetMyTickets);
            return await response.Content.ReadFromJsonAsync<IEnumerable<TicketListViewModel>>() ?? throw new Exception("Failed to deserialize response");
        }

        public async Task<IEnumerable<TicketListViewModel>> GetMyClosedTicketsAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SupportTicketGetMyClosed);
            return await response.Content.ReadFromJsonAsync<IEnumerable<TicketListViewModel>>() ?? new List<TicketListViewModel>();
        }

        public async Task<ApiResponse<SupportTicket>> CreateWithdrawalTicketAsync(CreateWithdrawalRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketCreateWithdrawal, request);
                return await HandleResponse<SupportTicket>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupportTicket>.ErrorResponse($"Error creating withdrawal ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SupportTicket>> CreatePurchaseRequestTicketAsync(CreatePurchaseRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketCreatePurchase, request);
                return await HandleResponse<SupportTicket>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreatePurchaseRequestTicketAsync: {ex.Message}");
                return ApiResponse<SupportTicket>.ErrorResponse($"เกิดข้อผิดพลาดในการสร้างคำขอจัดซื้อ: {ex.Message}");
            }
        }

        public async Task<IEnumerable<TicketListViewModel>> GetAllTicketsAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SupportTicketGetAll);
            return await response.Content.ReadFromJsonAsync<IEnumerable<TicketListViewModel>>()
                   ?? throw new Exception("Failed to deserialize response");
        }

        public async Task<TicketDetailViewModel> GetTicketDetailsAsync(int ticketId)
        {
            var endpoint = string.Format(_apiSettings.SupportTicketGetDetails, ticketId);
            var response = await _httpClient.GetAsync(endpoint);
            return await response.Content.ReadFromJsonAsync<TicketDetailViewModel>() ?? throw new Exception("Failed to deserialize response");
        }
        public async Task<ApiResponse<bool>> AcceptTicketAsync(TicketActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketAccept, request);
            return await HandleResponse<bool>(response);
        }

        public async Task<ApiResponse<bool>> ResolveTicketAsync(TicketActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketResolve, request);
            return await HandleResponse<bool>(response);
        }

        public async Task<ApiResponse<bool>> CloseTicketAsync(TicketActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketClose, request);
            return await HandleResponse<bool>(response);
        }

        public async Task<ApiResponse<bool>> CancelTicketAsync(TicketActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketCancel, request);
            return await HandleResponse<bool>(response);
        }

        public async Task<ApiResponse<bool>> RejectTicketAsync(TicketActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketReject, request);
            return await HandleResponse<bool>(response);
        }
    }
}