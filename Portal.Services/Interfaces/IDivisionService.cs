// FileName: Portal.Services/Interfaces/IDivisionService.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface IDivisionService
    {
        Task<IEnumerable<DivisionViewModel>> GetAllAsync();
        Task<DivisionViewModel> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentViewModel>> GetDepartmentsByDivisionIdAsync(int divisionId);
        Task<ApiResponse> CreateAsync(DivisionViewModel viewModel);
        Task<ApiResponse> UpdateAsync(int id, DivisionViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
    }
}