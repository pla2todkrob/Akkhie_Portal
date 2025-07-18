using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface ICompanyRequest
    {
        Task<IEnumerable<CompanyViewModel>> GetAllAsync();
        Task<CompanyViewModel> GetByIdAsync(int id);
        Task<IEnumerable<CompanyBranchViewModel>> GetBranchesByCompanyIdAsync(int companyId);
        Task<IEnumerable<DivisionViewModel>> GetDivisionsByCompanyIdAsync(int companyId);
        Task<ApiResponse<object>> CreateAsync(CompanyViewModel viewModel);
        Task<ApiResponse<object>> UpdateAsync(int id, CompanyViewModel viewModel);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}