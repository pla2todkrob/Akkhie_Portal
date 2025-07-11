using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Services.Interfaces
{
    public interface ISupportCategoryService
    {
        Task<IEnumerable<SupportCategoryViewModel>> GetAllAsync();
        Task<SupportCategoryViewModel?> GetByIdAsync(int id);
        Task<SupportCategoryViewModel> CreateAsync(SupportCategoryViewModel model);
        Task<SupportCategoryViewModel> UpdateAsync(SupportCategoryViewModel model);
        Task<bool> DeleteAsync(int id);
    }
}
