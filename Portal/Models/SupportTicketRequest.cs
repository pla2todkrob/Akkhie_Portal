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
                // Log the exception (using a proper logger in a real app)
                return ApiResponse<SupportTicket>.ErrorResponse($"Error creating ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<SupportTicketCategory>>> GetCategoriesAsync(string categoryType)
        {
            try
            {
                var endpoint = $"{_apiSettings.SupportTicketGetCategories}?categoryType={categoryType}";
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<IEnumerable<SupportTicketCategory>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SupportTicketCategory>>.ErrorResponse($"Error fetching categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<TicketListViewModel>>> GetMyTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.SupportTicketGetMyTickets);
                return await HandleResponse<IEnumerable<TicketListViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<TicketListViewModel>>.ErrorResponse($"Error fetching my tickets: {ex.Message}");
            }
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

        public async Task<ApiResponse<IEnumerable<TicketListViewModel>>> GetAllTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.SupportTicketGetAll);
                return await HandleResponse<IEnumerable<TicketListViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<TicketListViewModel>>.ErrorResponse($"Error fetching all tickets: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketDetailViewModel>> GetTicketDetailsAsync(int ticketId)
        {
            try
            {
                var endpoint = string.Format(_apiSettings.SupportTicketGetDetails, ticketId);
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<TicketDetailViewModel>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<TicketDetailViewModel>.ErrorResponse($"Error fetching ticket details: {ex.Message}");
            }
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