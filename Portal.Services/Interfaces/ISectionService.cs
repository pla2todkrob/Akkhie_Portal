using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface ISectionService
    {
        Task<List<SectionViewModel>> AllAsync();

        Task<SectionViewModel> SearchAsync(int id);

        Task<List<SectionViewModel>> SearchByDepartmentAsync(int id);

        Task<SectionViewModel> SaveAsync(SectionViewModel model);

        Task<bool> DeleteAsync(int id);
    }
}
