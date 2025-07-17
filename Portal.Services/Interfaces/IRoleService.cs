using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();

        Task<Role?> GetRoleByIdAsync(int id);

        Task<Role> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(int id, Role role);

        Task<bool> DeleteRoleAsync(int id);
    }
}
