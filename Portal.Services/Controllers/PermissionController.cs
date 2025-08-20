using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using System.Security.Claims;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger) : ControllerBase
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
    }
}
