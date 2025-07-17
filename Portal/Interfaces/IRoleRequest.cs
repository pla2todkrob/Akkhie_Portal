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
    }
}
