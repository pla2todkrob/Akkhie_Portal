using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Interfaces
{
    public interface IRoleRequest
    {
        Task<IEnumerable<Role>> GetAllAsync();
    }
}