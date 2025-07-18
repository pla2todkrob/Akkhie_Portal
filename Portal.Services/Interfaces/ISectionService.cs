// FileName: Portal.Services/Interfaces/ISectionService.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface ISectionService
    {
        Task<IEnumerable<SectionViewModel>> GetAllAsync();
        Task<SectionViewModel> GetByIdAsync(int id);
        Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId);
        Task<ApiResponse<Section>> CreateAsync(SectionViewModel viewModel);
        Task<ApiResponse<Section>> UpdateAsync(int id, SectionViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
    }
}