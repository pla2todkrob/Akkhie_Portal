// FileName: Portal.Services/Models/RoleService.cs
using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class RoleService : IRoleService
    {
        private readonly PortalDbContext _context;

        public RoleService(PortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}