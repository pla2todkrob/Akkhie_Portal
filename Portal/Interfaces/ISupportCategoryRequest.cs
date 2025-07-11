using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Interfaces
{
    public interface ISupportCategoryRequest
    {
        Task<ApiResponse<IEnumerable<SupportCategoryViewModel>>> GetAllAsync();
        Task<ApiResponse<SupportCategoryViewModel>> GetByIdAsync(int id);
        Task<ApiResponse<SupportCategoryViewModel>> CreateAsync(SupportCategoryViewModel model);
        Task<ApiResponse<SupportCategoryViewModel>> UpdateAsync(SupportCategoryViewModel model);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
