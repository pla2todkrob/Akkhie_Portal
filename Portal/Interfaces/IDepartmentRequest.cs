using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IDepartmentRequest
    {
        Task<IEnumerable<DepartmentViewModel>> GetAllAsync();
        Task<DepartmentViewModel> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentViewModel>> GetByDivisionIdAsync(int divisionId);
        Task<ApiResponse<object>> CreateAsync(DepartmentViewModel viewModel);
        Task<ApiResponse<object>> UpdateAsync(int id, DepartmentViewModel viewModel);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}