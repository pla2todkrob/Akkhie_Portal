using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface IDivisionService
    {
        Task<IEnumerable<DivisionViewModel>> GetAllAsync();
        Task<DivisionViewModel> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(DivisionViewModel viewModel);
        Task<ApiResponse> UpdateAsync(int id, DivisionViewModel viewModel);
        Task<ApiResponse> DeleteAsync(int id);
    }
}
