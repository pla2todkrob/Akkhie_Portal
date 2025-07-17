using Portal.Models;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Interfaces
{
    /// <summary>
    /// Interface สำหรับจัดการการร้องขอข้อมูล Role ไปยัง API
    /// </summary>
    public interface IRoleRequest
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetByIdAsync(int id);
        Task<Role> CreateAsync(RoleRequest role);
        Task<bool> UpdateAsync(int id, RoleRequest role);
        Task<bool> DeleteAsync(int id);
    }
}
