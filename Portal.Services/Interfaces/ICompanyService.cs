using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<List<CompanyViewModel>> AllAsync();

        Task<CompanyViewModel?> SearchAsync(int id);

        Task<CompanyViewModel> SaveAsync(CompanyViewModel companyViewModel);

        Task<bool> DeleteAsync(int id);

        Task<List<CompanyBranchViewModel>> SearchBranchesByCompany(int id);
    }
}
