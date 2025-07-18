using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IDivisionRequest
    {
        Task<IEnumerable<DivisionViewModel>> GetAllAsync();
        Task<DivisionViewModel> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentViewModel>> GetDepartmentsByDivisionIdAsync(int divisionId);
        Task<ApiResponse<object>> CreateAsync(DivisionViewModel viewModel);
        Task<ApiResponse<object>> UpdateAsync(int id, DivisionViewModel viewModel);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}