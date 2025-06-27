using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class SectionController(
        ILogger<AuthController> logger,
        ISectionRequest sectionRequest,
        IDivisionRequest divisionRequest,
        IDepartmentRequest departmentRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var response = await sectionRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get sections: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No sections found.");
                return NotFound("No sections found.");
            }
            return View(response.Data);
        }

        // GET: Section/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new SectionViewModel());
        }

        // POST: Section/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse.ErrorResponse("ข้อมูลไม่ถูกต้อง", errors));
            }

            try
            {
                var response = await sectionRequest.SaveAsync(model);
                if (!response.Success)
                {
                    logger.LogError("Failed to save section: {Message}", response.Message);
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving section");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }

        }

        // GET: Section/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid section ID: {Id}", id);
                return BadRequest("Invalid section ID.");
            }


            var section = await sectionRequest.SearchAsync(id);
            if (!section.Success)
            {
                logger.LogError("Failed to get section: {Message}", section.Message);
                return NotFound(section.Message);
            }
            if (section.Data == null)
            {
                logger.LogWarning("Section with ID {Id} not found.", id);
                return NotFound($"Section with ID {id} not found.");
            }

            await PopulateDropdownsAsync(section.Data.DivisionId, section.Data.DepartmentId);
            return View(section.Data);
        }

        // POST: Section/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse.ErrorResponse("ข้อมูลไม่ถูกต้อง", errors));
            }
            try
            {
                var response = await sectionRequest.SaveAsync(model);
                if (!response.Success)
                {
                    logger.LogError("Failed to update section: {Message}", response.Message);
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating section");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid section ID: {id}", id);
                return BadRequest("Invalid section ID.");
            }
            var response = await sectionRequest.DeleteAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to delete section {id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        private async Task PopulateDropdownsAsync(int? divisionId = null, int? departmentId = null)
        {
            // 1. ดึงข้อมูล Division ทั้งหมดมาแสดงใน Dropdown แรกเสมอ
            var allDivisions = await divisionRequest.AllAsync();
            ViewBag.Divisions = allDivisions.Data?.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = d.Id == divisionId && divisionId.HasValue
            }) ?? [];

            // 2. ถ้ามี divisionId ถูกเลือกไว้, ให้ดึง Department ที่เกี่ยวข้อง
            if (divisionId.HasValue)
            {
                var allDepartments = await departmentRequest.SearchByDivision(divisionId.Value);
                ViewBag.Departments = allDepartments.Data?.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == departmentId && departmentId.HasValue
                }) ?? [];
            }
            else
            {
                ViewBag.Departments = new List<SelectListItem>();
            }
        }
    }
}
