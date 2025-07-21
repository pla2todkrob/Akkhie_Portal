using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Models;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class DivisionController(IDivisionRequest divisionRequest, ICompanyRequest companyRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var divisions = await divisionRequest.GetAllAsync();
            return View(divisions);
        }

        public async Task<IActionResult> Departments(int id)
        {
            var departments = await divisionRequest.GetDepartmentsByDivisionIdAsync(id);
            return PartialView("_Departments", departments);
        }

        public async Task<IActionResult> Create()
        {
            // ดึงข้อมูลบริษัททั้งหมด
            var companies = await companyRequest.GetAllAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            var model = new DivisionViewModel
            {
                DepartmentViewModels = [new()]
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DivisionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await divisionRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var companies = await companyRequest.GetAllAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            var division = await divisionRequest.GetByIdAsync(id);
            if (division == null)
            {
                return NotFound();
            }
            return View(division);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DivisionViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await divisionRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await divisionRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(new { success = true, message = "ลบข้อมูลสำเร็จ" });
            }
            return BadRequest(new { success = false, message = response.Message ?? "เกิดข้อผิดพลาดในการลบข้อมูล" });
        }
    }
}
