using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IDivisionRequest
    {
        Task<ApiResponse<IEnumerable<DivisionViewModel>>> AllAsync();

        Task<ApiResponse<DivisionViewModel>> SearchAsync(int id);

        Task<ApiResponse<object>> SaveAsync(DivisionViewModel model);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
