using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface ISectionRequest
    {
        Task<IEnumerable<SectionViewModel>> GetAllAsync();
        Task<SectionViewModel> GetByIdAsync(int id);
        Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId);
        Task<ApiResponse<object>> CreateAsync(SectionViewModel viewModel);
        Task<ApiResponse<object>> UpdateAsync(int id, SectionViewModel viewModel);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}