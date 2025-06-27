using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface IDivisionService
    {
        Task<List<DivisionViewModel>> AllAsync();

        Task<DivisionViewModel> SearchAsync(int id);

        Task<DivisionViewModel> SaveAsync(DivisionViewModel division);

        Task<bool> DeleteAsync(int id);
    }
}
