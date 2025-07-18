// FileName: Portal.Services/Interfaces/IRoleService.cs
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllAsync();
    }
}