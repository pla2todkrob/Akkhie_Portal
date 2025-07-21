using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class DepartmentController(IDepartmentRequest departmentRequest, ICompanyRequest companyRequest, IDivisionRequest divisionRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var departments = await departmentRequest.GetAllAsync();
            return View(departments);
        }

        // Helper method สำหรับจัดการ Dropdowns ทั้งหมด
        private async Task PopulateDropdowns(DepartmentViewModel model = null)
        {
            var companies = await companyRequest.GetAllAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name", model?.CompanyId);

            if (model?.CompanyId > 0)
            {
                // ถ้ามี CompanyId, ให้ดึง Division ที่เกี่ยวข้องมาแสดง
                var allDivisions = await divisionRequest.GetAllAsync(); // หมายเหตุ: ควรมี API ที่กรองได้ดีกว่านี้
                var filteredDivisions = allDivisions.Where(d => d.CompanyId == model.CompanyId);
                ViewBag.Divisions = new SelectList(filteredDivisions, "Id", "Name", model?.DivisionId);
            }
            else
            {
                // ถ้ายังไม่ได้เลือก Company, ให้ Dropdown ของ Division เป็นค่าว่าง
                ViewBag.Divisions = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Name");
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var model = new DepartmentViewModel { SectionViewModels = new List<SectionViewModel> { new() } };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await departmentRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model); // ส่ง Dropdown กลับไปอีกครั้งเมื่อเกิด Error
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await departmentRequest.GetByIdAsync(id);
            if (department == null) return NotFound();

            await PopulateDropdowns(department);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var response = await departmentRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model); // ส่ง Dropdown กลับไปอีกครั้งเมื่อเกิด Error
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await departmentRequest.DeleteAsync(id);
            return Ok(response);
        }

        public async Task<IActionResult> Sections(int id)
        {
            var sections = await departmentRequest.GetSectionsByDepartmentIdAsync(id);
            return PartialView("_Sections", sections);
        }
    }
}
