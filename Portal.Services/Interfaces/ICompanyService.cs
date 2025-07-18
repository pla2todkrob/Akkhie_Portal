// FileName: Portal.Services/Interfaces/ICompanyService.cs
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyViewModel>> GetAllAsync();
        Task<CompanyViewModel> GetByIdAsync(int id);
        Task<IEnumerable<CompanyBranchViewModel>> GetBranchesByCompanyIdAsync(int companyId);
        Task<ApiResponse<Company>> CreateAsync(CompanyViewModel viewModel);
        Task<ApiResponse<Company>> UpdateAsync(int id, CompanyViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
    }
}