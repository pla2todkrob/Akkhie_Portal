// File: Portal/Models/SupportTicketRequest.cs
using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Models
{
    public class SupportTicketRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        : BaseRequest(httpClient, apiSettings), ISupportTicketRequest
    {
        public async Task<ApiResponse<SupportTicket>> CreateTicketAsync(CreateTicketRequest model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.SupportTicketCreate, model);
                return await HandleResponse<SupportTicket>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SupportTicket>.ErrorResponse($"Error creating ticket: {ex.Message}");
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
                // In a real app, you would use a proper logger.
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

    }
}