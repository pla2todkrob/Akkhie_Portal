using Portal.Shared.Models.ViewModel.IT_Inventory;

namespace Portal.Services.Interfaces
{
    public interface IITInventoryService
    {
        /// <summary>
        /// Gets a list of all stock items that are available for withdrawal.
        /// </summary>
        Task<IEnumerable<StockItemViewModel>> GetAvailableStockItemsAsync();
    }
}
