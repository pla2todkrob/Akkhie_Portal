using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyRequest _companyRequest;

        public CompanyController(ICompanyRequest companyRequest)
        {
            _companyRequest = companyRequest;
        }

        // [FINAL FIX] ปรับแก้ Controller ให้เรียกใช้เมธอดจาก Interface ฉบับล่าสุด
        public async Task<IActionResult> Index()
        {
            var companies = await _companyRequest.GetAllAsync();
            return View(companies);
        }

        // Action สำหรับแสดงรายการสาขาใน Modal
        public async Task<IActionResult> Branches(int id)
        {
            var branches = await _companyRequest.GetBranchesByCompanyIdAsync(id);
            return PartialView("_Branches", branches);
        }

        public IActionResult Create()
        {
            var model = new CompanyViewModel
            {
                // เริ่มต้นด้วยสาขาว่าง 1 รายการสำหรับกรอกข้อมูล
                CompanyBranchViewModels = new List<CompanyBranchViewModel> { new() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _companyRequest.CreateAsync(model);
                if (response.Success)
                {
                    // ส่งผลลัพธ์กลับไปให้ AJAX ที่ฝั่ง Client
                    return Ok(new { success = true });
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            // ส่ง Model State ที่มี Error กลับไปให้ AJAX
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyRequest.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await _companyRequest.UpdateAsync(id, model);
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
            var response = await _companyRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(new { success = true, message = "ลบข้อมูลสำเร็จ" });
            }
            return BadRequest(new { success = false, message = response.Message ?? "เกิดข้อผิดพลาดในการลบข้อมูล" });
        }
    }
}
