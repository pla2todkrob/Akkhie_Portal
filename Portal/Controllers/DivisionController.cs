using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class DivisionController : Controller
    {
        private readonly IDivisionRequest _divisionRequest;

        public DivisionController(IDivisionRequest divisionRequest)
        {
            _divisionRequest = divisionRequest;
        }

        public async Task<IActionResult> Index()
        {
            var divisions = await _divisionRequest.GetAllAsync();
            return View(divisions);
        }

        public IActionResult Create()
        {
            var model = new DivisionViewModel
            {
                DepartmentViewModels = new List<DepartmentViewModel> { new() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DivisionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await _divisionRequest.CreateAsync(model);
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
            var division = await _divisionRequest.GetByIdAsync(id);
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
                // [FIX] ตรวจสอบ response.Success จาก ApiResponse<object>
                var response = await _divisionRequest.UpdateAsync(id, model);
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
            // [FIX] เปลี่ยน Delete ให้รองรับการเรียกผ่าน AJAX และคืนค่าเป็น JSON
            var response = await _divisionRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(new { success = true, message = "ลบข้อมูลสำเร็จ" });
            }
            return BadRequest(new { success = false, message = response.Message ?? "เกิดข้อผิดพลาดในการลบข้อมูล" });
        }
    }
}
