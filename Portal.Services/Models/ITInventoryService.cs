using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.ViewModel.IT_Inventory;

namespace Portal.Services.Models
{
    public class ITInventoryService(PortalDbContext context) : IITInventoryService
    {
        public async Task<IEnumerable<StockItemViewModel>> GetAvailableStockItemsAsync()
        {
            return await context.IT_Stocks
                .AsNoTracking()
                .Include(s => s.Item)
                .Where(s => s.Quantity > 0 && s.Item.IsStockItem)
                .Select(s => new StockItemViewModel
                {
                    StockId = s.Id,
                    ItemId = s.ItemId,
                    Name = s.Item.Name,
                    ItemType = s.Item.ItemType,
                    Specification = s.Item.Specification,
                    Quantity = s.Quantity
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}
