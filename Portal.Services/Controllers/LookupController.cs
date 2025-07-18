using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Services.Models;

namespace Portal.Services.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController(PortalDbContext context) : ControllerBase
    {
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var items = await context.Companies
                                      .Select(c => new { value = c.Id, text = c.Name })
                                      .ToListAsync();
            return Ok(items);
        }

        [HttpGet("divisions/company/{companyId}")]
        public async Task<IActionResult> GetDivisionsByCompany(int companyId)
        {
            var items = await context.Divisions
                                      .Where(d => d.CompanyId == companyId)
                                      .Select(d => new { value = d.Id, text = d.Name })
                                      .ToListAsync();
            return Ok(items);
        }

        [HttpGet("departments/division/{divisionId}")]
        public async Task<IActionResult> GetDepartmentsByDivision(int divisionId)
        {
            var items = await context.Departments
                                        .Where(d => d.DivisionId == divisionId)
                                        .Select(d => new { value = d.Id, text = d.Name })
                                        .ToListAsync();
            return Ok(items);
        }

        [HttpGet("sections/department/{departmentId}")]
        public async Task<IActionResult> GetSectionsByDepartment(int departmentId)
        {
            var items = await context.Sections
                                     .Where(s => s.DepartmentId == departmentId)
                                     .Select(s => new { value = s.Id, text = s.Name })
                                     .ToListAsync();
            return Ok(items);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var items = await context.Roles
                                      .Select(r => new { value = r.Id, text = r.Name })
                                      .ToListAsync();
            return Ok(items);
        }
    }
}