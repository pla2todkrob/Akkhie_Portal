using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Models;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;
using System.DirectoryServices.Protocols;

namespace Portal.Controllers
{
    public class DivisionController(ILogger<AuthController> logger, IDivisionRequest divisionRequest, IDepartmentRequest departmentRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var response = await divisionRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get divisions: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No divisions found.");
                return NotFound("No divisions found.");
            }
            return View(response.Data);
        }

        public async Task<IActionResult> GetDepartments(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid division ID: {id}", id);
                return BadRequest("Invalid division ID.");
            }
            var response = await departmentRequest.SearchByDivision(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get departments for division {DivisionId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No departments found for division {DivisionId}.", id);
                return NotFound($"No departments found for division ID {id}.");
            }
            return PartialView("_Departments", response.Data);
        }

        public IActionResult Create()
        {
            return View(new DivisionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DivisionViewModel model)
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
                var response = await divisionRequest.SaveAsync(model);

                if (!response.Success)
                {
                    logger.LogError("Failed to save division: {Message}", response.Message);
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving division");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid division ID: {id}", id);
                return BadRequest("Invalid division ID.");
            }
            var response = await divisionRequest.SearchAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get division {DivisionId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null)
            {
                logger.LogWarning("Division with ID {DivisionId} not found.", id);
                return NotFound($"Division with ID {id} not found.");
            }
            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DivisionViewModel model)
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
                var response = await divisionRequest.SaveAsync(model);
                if (!response.Success)
                {
                    logger.LogError("Failed to update division: {Message}", response.Message);
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating division");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid division ID: {id}", id);
                return BadRequest("Invalid division ID.");
            }
            var response = await divisionRequest.DeleteAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to delete division {DivisionId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
