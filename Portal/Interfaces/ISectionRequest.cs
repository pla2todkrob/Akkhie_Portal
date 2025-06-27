using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface ISectionRequest
    {
        Task<ApiResponse<IEnumerable<SectionViewModel>>> AllAsync();

        Task<ApiResponse<SectionViewModel>> SearchAsync(int id);

        Task<ApiResponse<IEnumerable<SectionViewModel>>> SearchByDepartment(int departmentId);

        Task<ApiResponse<SectionViewModel>> SaveAsync(SectionViewModel model);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
