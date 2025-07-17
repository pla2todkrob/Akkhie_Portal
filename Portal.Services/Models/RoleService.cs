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

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> UpdateRoleAsync(int id, Role role)
        {
            if (id != role.Id)
            {
                return false;
            }

            var existingRole = await context.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return false;
            }

            // Update properties
            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            context.Roles.Remove(role);
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> RoleExists(int id)
        {
            return await context.Roles.AnyAsync(e => e.Id == id);
        }
    }
}
