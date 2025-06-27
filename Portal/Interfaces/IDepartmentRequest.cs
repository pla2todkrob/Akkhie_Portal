using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IDepartmentRequest
    {
        Task<ApiResponse<IEnumerable<DepartmentViewModel>>> AllAsync();

        Task<ApiResponse<IEnumerable<DepartmentViewModel>>> SearchByDivision(int divisionId);

        Task<ApiResponse<DepartmentViewModel>> SearchAsync(int id);

        Task<ApiResponse<DepartmentViewModel>> SaveAsync(DepartmentViewModel model);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
