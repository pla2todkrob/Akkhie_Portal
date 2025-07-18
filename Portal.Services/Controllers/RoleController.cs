using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService, ILogger<RoleController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetRoles()
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
    }
}
