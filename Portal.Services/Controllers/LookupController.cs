using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LookupController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IDivisionService _divisionService;
        private readonly IDepartmentService _departmentService;
        private readonly ISectionService _sectionService;
        private readonly IRoleService _roleService;

        public LookupController(
            ICompanyService companyService,
            IDivisionService divisionService,
            IDepartmentService departmentService,
            ISectionService sectionService,
            IRoleService roleService)
        {
            _companyService = companyService;
            _divisionService = divisionService;
            _departmentService = departmentService;
            _sectionService = sectionService;
            _roleService = roleService;
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _companyService.GetAllAsync();
            var selectList = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return Ok(selectList);
        }

        [HttpGet("branches/company/{companyId}")]
        public async Task<IActionResult> GetBranchesByCompany(int companyId)
        {
            var branches = await _companyService.GetBranchesByCompanyIdAsync(companyId);
            var selectList = branches.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            return Ok(selectList);
        }

        // [FINAL FIX] แก้ไขให้เรียกใช้เมธอด GetByCompanyIdAsync ที่ถูกต้อง
        [HttpGet("divisions/company/{companyId}")]
        public async Task<IActionResult> GetDivisionsByCompany(int companyId)
        {
            var divisions = await _divisionService.GetByCompanyIdAsync(companyId);
            var selectList = divisions.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Ok(selectList);
        }

        [HttpGet("departments/division/{divisionId}")]
        public async Task<IActionResult> GetDepartmentsByDivision(int divisionId)
        {
            var departments = await _departmentService.GetByDivisionIdAsync(divisionId);
            var selectList = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Ok(selectList);
        }

        [HttpGet("sections/department/{departmentId}")]
        public async Task<IActionResult> GetSectionsByDepartment(int departmentId)
        {
            var sections = await _sectionService.GetByDepartmentIdAsync(departmentId);
            var selectList = sections.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            return Ok(selectList);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetAllAsync();
            var selectList = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            return Ok(selectList);
        }
    }
}
