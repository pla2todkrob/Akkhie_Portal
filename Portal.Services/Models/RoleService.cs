using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Data;

namespace Portal.Services.Models
{
    public class RoleService(PortalDbContext context) : IRoleService
    {
        public async Task<List<Role>> AllAsync()
        {
            return await context.Roles.AsNoTracking().ToListAsync();
        }

        public async Task<Role?> SearchAsync(int id)
        {
            return await context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> SaveAsync(Role role)
        {
            ArgumentNullException.ThrowIfNull(role);
            if (role.Id == 0)
            {
                context.Roles.Add(role);
            }
            else
            {
                var existingRole = await context.Roles.FindAsync(role.Id) ?? throw new DataException("ไม่พบบทบาทที่ต้องการบันทึก");

                existingRole.Name = role.Name;
                existingRole.Description = role.Description;
            }
            await context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return false;

            context.Roles.Remove(role);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
