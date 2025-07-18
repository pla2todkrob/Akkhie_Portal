// FileName: Portal.Services/Controllers/RoleController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _roleService.GetAllAsync();
            return Ok(result);
        }
    }
}