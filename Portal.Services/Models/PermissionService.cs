using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using System.Security.Claims;

namespace Portal.Services.Models
{
    public class PermissionService : IPermissionService
    {
        private readonly PortalDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PermissionService(PortalDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> HasPermissionAsync(string permissionKey)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue) return false;

            var userPermissions = await GetUserPermissionsAsync(userId.Value);
            return userPermissions.Contains(permissionKey);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
        {
            var user = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (user == null) return new HashSet<string>();

            // Rule พิเศษ: SystemRole มีทุกสิทธิ์
            if (user.IsSystemRole)
            {
                return await _context.Permissions.Select(p => p.Key).ToHashSetAsync();
            }

            // 1. ดึงสิทธิ์ที่ผูกกับ Role ของ User
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == user.RoleId)
                .Select(rp => rp.Permission.Key)
                .ToListAsync();

            // 2. ดึงสิทธิ์ที่ผูกกับ User โดยตรง (สำหรับกรณีพิเศษ)
            var userPermissions = await _context.EmployeePermissions
                .Where(ep => ep.EmployeeId == user.Id)
                .Select(ep => ep.Permission.Key)
                .ToListAsync();

            // รวมสิทธิ์ทั้งหมด
            var allPermissions = new HashSet<string>(rolePermissions);
            allPermissions.UnionWith(userPermissions);

            return allPermissions;
        }
    }
}