using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class DepartmentController(ILogger<AuthController> logger, IDivisionRequest divisionRequest, IDepartmentRequest departmentRequest, ISectionRequest sectionRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var response = await departmentRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get departments: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No departments found.");
                return NotFound("No departments found.");
            }
            return View(response.Data);
        }

        public async Task<IActionResult> GetSections(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid department ID: {id}", id);
                return BadRequest("Invalid department ID.");
            }
            var response = await sectionRequest.SearchByDepartment(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get sections for department {id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No sections found for department {id}.", id);
                return NotFound($"No sections found for department ID {id}.");
            }
            return PartialView("_Sections", response.Data);
        }

        public async Task<IActionResult> Create()
        {
            var divisions = await divisionRequest.AllAsync();
            ViewData["Divisions"] = divisions.Data?.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }) ?? [];
            return View(new DepartmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
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
                var response = await departmentRequest.SaveAsync(model);
                if (!response.Success)
                {
                    logger.LogError("Failed to save department: {Message}", response.Message);
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving department");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid department ID: {id}", id);
                return BadRequest("Invalid department ID.");
            }
            var response = await departmentRequest.SearchAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get department {id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null)
            {
                logger.LogWarning("Department with ID {id} not found.", id);
                return NotFound($"Department with ID {id} not found.");
            }

            var divisions = await divisionRequest.AllAsync();
            ViewData["Divisions"] = divisions.Data?.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = d.Id == response.Data.DivisionId
            }) ?? [];

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentViewModel model)
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
                var response = await departmentRequest.SaveAsync(model);
                if (!response.Success)
                {
                    logger.LogError("Failed to update department: {Message}", response.Message);
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating department");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid department ID: {id}", id);
                return BadRequest("Invalid department ID.");
            }
            var response = await departmentRequest.DeleteAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to delete department {id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
