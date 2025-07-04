using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.IT_Inventory;

namespace Portal.Interfaces
{
    public interface IITInventoryRequest
    {
        /// <summary>
        /// Gets a list of all available stock items from the API.
        /// </summary>
        Task<ApiResponse<IEnumerable<StockItemViewModel>>> GetAvailableStockItemsAsync();
    }
}
