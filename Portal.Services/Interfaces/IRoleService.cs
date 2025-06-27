using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> AllAsync();

        Task<Role?> SearchAsync(int id);

        Task<Role> SaveAsync(Role role);

        Task<bool> DeleteAsync(int id);
    }
}
