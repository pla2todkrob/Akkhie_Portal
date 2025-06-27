using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface ICompanyRequest
    {
        Task<ApiResponse<IEnumerable<CompanyViewModel>>> AllAsync();

        Task<ApiResponse<IEnumerable<CompanyBranchViewModel>>> GetBranchesByCompanyAsync(int companyId);

        Task<ApiResponse<object>> SaveAsync(CompanyViewModel model);

        Task<ApiResponse<CompanyViewModel>> SearchAsync(int id);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
