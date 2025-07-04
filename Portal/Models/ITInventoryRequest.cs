using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.IT_Inventory;

namespace Portal.Models
{
    /// <summary>
    /// Implementation for making requests to the IT Inventory API endpoints.
    /// </summary>
    public class ITInventoryRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        : BaseRequest(httpClient, apiSettings), IITInventoryRequest
    {
        /// <inheritdoc/>
        public async Task<ApiResponse<IEnumerable<StockItemViewModel>>> GetAvailableStockItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiSettings.ITStockItemsGetAvailable);
                return await HandleResponse<IEnumerable<StockItemViewModel>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<StockItemViewModel>>.ErrorResponse($"Error fetching stock items: {ex.Message}");
            }
        }
    }
}
