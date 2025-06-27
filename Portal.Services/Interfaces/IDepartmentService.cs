using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentViewModel>> AllAsync();

        Task<DepartmentViewModel> SearchAsync(int id);

        Task<List<DepartmentViewModel>> SearchByDivisionAsync(int divisionId);

        Task<DepartmentViewModel> SaveAsync(DepartmentViewModel model);

        Task<bool> DeleteAsync(int id);
    }
}
