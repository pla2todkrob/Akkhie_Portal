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
    }
}