using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ต้องมีการยืนยันตัวตนก่อนใช้งาน
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return Ok(await roleService.GetAllRolesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            var createdRole = await roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, Role role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }

            var result = await roleService.UpdateRoleAsync(id, role);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
