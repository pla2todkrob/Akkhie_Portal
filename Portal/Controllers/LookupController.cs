using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;

namespace Portal.Controllers
{
    public class LookupController : Controller
    {
        private readonly ICompanyRequest _companyRequest;
        private readonly IDivisionRequest _divisionRequest;
        private readonly IDepartmentRequest _departmentRequest;
        private readonly ISectionRequest _sectionRequest;
        private readonly IRoleRequest _roleRequest;

        public LookupController(
            ICompanyRequest companyRequest,
            IDivisionRequest divisionRequest,
            IDepartmentRequest departmentRequest,
            ISectionRequest sectionRequest,
            IRoleRequest roleRequest)
        {
            _companyRequest = companyRequest;
            _divisionRequest = divisionRequest;
            _departmentRequest = departmentRequest;
            _sectionRequest = sectionRequest;
            _roleRequest = roleRequest;
        }

        // [FINAL FIX] ปรับแก้ Controller ทั้งหมดให้เรียกใช้เมธอดจาก Interface ที่เป็นมาตรฐานเดียวกัน
        // และแปลงผลลัพธ์ (ViewModel) เป็น SelectListItem ก่อนส่งกลับเป็น JSON

        [HttpGet]
        public async Task<JsonResult> GetCompanies()
        {
            var companies = await _companyRequest.GetAllAsync();
            var selectList = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetBranchesByCompany(int id)
        {
            var branches = await _companyRequest.GetBranchesByCompanyIdAsync(id);
            var selectList = branches.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetDivisions()
        {
            var divisions = await _divisionRequest.GetAllAsync();
            var selectList = divisions.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByDivision(int id)
        {
            var departments = await _departmentRequest.GetByDivisionIdAsync(id);
            var selectList = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsByDepartment(int id)
        {
            var sections = await _sectionRequest.GetByDepartmentIdAsync(id);
            var selectList = sections.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            return Json(selectList);
        }

        [HttpGet]
        public async Task<JsonResult> GetRoles()
        {
            var roles = await _roleRequest.GetAllAsync();
            var selectList = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            return Json(selectList);
        }
    }
}
