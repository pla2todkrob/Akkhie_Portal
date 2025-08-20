using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.IT_Inventory;

namespace Portal.Interfaces
{
    public interface IITInventoryRequest
    {
        Task<ApiResponse<IEnumerable<StockItemViewModel>>> GetAvailableStockItemsAsync();
    }
}
