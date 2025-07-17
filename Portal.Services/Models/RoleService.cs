using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class RoleService(PortalDbContext context) : IRoleService
    {
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await context.Roles.AsNoTracking().ToListAsync();
        }
    }
}
