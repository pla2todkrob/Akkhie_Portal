using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Net;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService, ILogger<RoleController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await roleService.AllAsync();
                return Ok(ApiResponse.SuccessResponse(roles, "Roles retrieved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving role list");
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> SearchRoleById(int id)
        {
            try
            {
                var role = await roleService.SearchAsync(id);
                if (role == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Role not found"));
                }
                return Ok(ApiResponse.SuccessResponse(role, "Role retrieved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving role by id");
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveRole([FromBody] Role role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Invalid role data", [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]));
                }
                await roleService.SaveAsync(role);
                return Ok(ApiResponse.SuccessResponse(role, "Role saved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving role");
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await roleService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Role not found"));
                }

                return Ok(ApiResponse.SuccessResponse(new { }, "Role deleted successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting role");
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }
    }
}
