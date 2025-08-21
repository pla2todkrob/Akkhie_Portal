using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Services.Models;
using Portal.Shared.Models.Entities;
using System.Security.Claims;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController(IPermissionService permissionService, PortalDbContext context, ILogger<PermissionController> logger) : ControllerBase
    {
        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out var userId))
                {
                    return Unauthorized("Invalid user identifier.");
                }

                var permissions = await permissionService.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching user permissions.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await context.Permissions.AsNoTracking().OrderBy(p => p.Key).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permission = await context.Permissions.FindAsync(id);
            return permission == null ? NotFound() : Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Permission permission)
        {
            if (await context.Permissions.AnyAsync(p => p.Key == permission.Key))
            {
                return BadRequest("Permission key already exists.");
            }
            context.Permissions.Add(permission);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }
            context.Entry(permission).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}