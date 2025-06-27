using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Interfaces
{
    public interface IRoleRequest
    {
        Task<ApiResponse<IEnumerable<Role>>> AllAsync();
    }
}
