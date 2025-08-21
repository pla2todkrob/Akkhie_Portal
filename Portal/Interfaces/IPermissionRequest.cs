using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Interfaces
{
    public interface IPermissionRequest
    {
        Task<HashSet<string>?> GetMyPermissionsAsync();
        Task<List<Permission>> GetAllAsync();
        Task<Permission> GetByIdAsync(int id);
        Task<ApiResponse<Permission>> CreateAsync(Permission model);
        Task<ApiResponse> UpdateAsync(int id, Permission model);
        Task<ApiResponse> DeleteAsync(int id);
    }
}