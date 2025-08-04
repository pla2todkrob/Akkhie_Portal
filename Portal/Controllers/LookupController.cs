using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class LookupController(
        ICompanyRequest companyRequest,
        IDivisionRequest divisionRequest,
        IDepartmentRequest departmentRequest,
        ISectionRequest sectionRequest,
        IRoleRequest roleRequest,
        ISupportTicketRequest support
        ) : Controller
    {
        [HttpGet]
        public async Task<JsonResult> GetCompanies()
        {
            var companies = await companyRequest.GetAllAsync();
            var selectList = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetBranchesByCompany(int id)
        {
            var branches = await companyRequest.GetBranchesByCompanyIdAsync(id);
            var selectList = branches.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetDivisionsByCompany(int companyId)
        {
            var divisions = await companyRequest.GetDivisionsByCompanyIdAsync(companyId);
            var selectList = divisions.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByDivision(int id)
        {
            var departments = await divisionRequest.GetDepartmentsByDivisionIdAsync(id);
            var selectList = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsByDepartment(int id)
        {
            var sections = await departmentRequest.GetSectionsByDepartmentIdAsync(id);
            var selectList = sections.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetRoles()
        {
            var roles = await roleRequest.GetAllAsync();
            var selectList = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetMyTickets()
        {
            var tickets = await support.GetMyTicketsAsync();
            var selectList = tickets.Select(t => new SelectListItem 
            { 
                Value = t.Id.ToString(), 
                Text = $"{t.Title} - {t.Status}" 
            });
            return Json(selectList);
        }
    }
}
