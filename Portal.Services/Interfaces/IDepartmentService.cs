// FileName: Portal.Services/Interfaces/IDepartmentService.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentViewModel>> GetAllAsync();
        Task<DepartmentViewModel> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentViewModel>> GetByDivisionIdAsync(int divisionId);
        Task<ApiResponse<Department>> CreateAsync(DepartmentViewModel viewModel);
        Task<ApiResponse<Department>> UpdateAsync(int id, DepartmentViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
    }
}