using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRequest _departmentRequest;
        private readonly IDivisionRequest _divisionRequest;

        public DepartmentController(IDepartmentRequest departmentRequest, IDivisionRequest divisionRequest)
        {
            _departmentRequest = departmentRequest;
            _divisionRequest = divisionRequest;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentRequest.GetAllAsync();
            return View(departments);
        }

        private async Task PopulateDivisionsDropDownList(object selectedDivision = null)
        {
            var divisions = await _divisionRequest.GetAllAsync();
            ViewBag.Divisions = new SelectList(divisions, "Id", "Name", selectedDivision);
        }

        public async Task<IActionResult> Create()
        {
            var model = new DepartmentViewModel
            {
                SectionViewModels = new List<SectionViewModel> { new() }
            };
            await PopulateDivisionsDropDownList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await _departmentRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDivisionsDropDownList(model.DivisionId);
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentRequest.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            await PopulateDivisionsDropDownList(department.DivisionId);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await _departmentRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDivisionsDropDownList(model.DivisionId);
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // [FIX] เปลี่ยน Delete ให้รองรับการเรียกผ่าน AJAX และคืนค่าเป็น JSON
            var response = await _departmentRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(new { success = true, message = "ลบข้อมูลสำเร็จ" });
            }
            return BadRequest(new { success = false, message = response.Message ?? "เกิดข้อผิดพลาดในการลบข้อมูล" });
        }
    }
}
