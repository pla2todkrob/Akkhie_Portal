using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class SectionController(ISectionRequest sectionRequest, IDivisionRequest divisionRequest, IDepartmentRequest departmentRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var sections = await sectionRequest.GetAllAsync();
            return View(sections);
        }

        private async Task PopulateDropdowns(int? divisionId = null, int? departmentId = null)
        {
            var divisions = await divisionRequest.GetAllAsync();
            ViewBag.Divisions = new SelectList(divisions, "Id", "Name", divisionId);

            if (divisionId.HasValue)
            {
                var departments = await divisionRequest.GetDepartmentsByDivisionIdAsync(divisionId.Value);
                ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);
            }
            else
            {
                ViewBag.Departments = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Name");
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await sectionRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model.DivisionId, model.DepartmentId);
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var section = await sectionRequest.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(section.DivisionId, section.DepartmentId);
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SectionViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await sectionRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model.DivisionId, model.DepartmentId);
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // [FIX] เปลี่ยน Delete ให้รองรับการเรียกผ่าน AJAX และคืนค่าเป็น JSON
            var response = await sectionRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(new { success = true, message = "ลบข้อมูลสำเร็จ" });
            }
            return BadRequest(new { success = false, message = response.Message ?? "เกิดข้อผิดพลาดในการลบข้อมูล" });
        }
    }
}
