using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> AllAsync();
    }
}
