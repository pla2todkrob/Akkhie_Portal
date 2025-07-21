// FileName: Portal/Interfaces/IDepartmentRequest.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Interfaces
{
    public interface IDepartmentRequest
    {
        Task<IEnumerable<DepartmentViewModel>> GetAllAsync();
        Task<DepartmentViewModel> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(DepartmentViewModel viewModel);
        Task<ApiResponse> UpdateAsync(int id, DepartmentViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
        Task<IEnumerable<SectionViewModel>> GetSectionsByDepartmentIdAsync(int departmentId);
    }
}