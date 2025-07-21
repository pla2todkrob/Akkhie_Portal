// FileName: Portal.Services/Controllers/RoleController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await roleService.GetAllAsync();
            return Ok(result);
        }
    }
}