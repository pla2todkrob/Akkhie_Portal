using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    [Authorize]
    public class CompanyController(ILogger<AuthController> logger, ICompanyRequest companyRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var response = await companyRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get companies: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No companies found.");
                return NotFound("No companies found.");
            }
            return View(response.Data);
        }

        public async Task<IActionResult> GetBranches(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid company ID: {Id}", id);
                return BadRequest("Invalid company ID.");
            }
            var response = await companyRequest.GetBranchesByCompanyAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get branches for company {Id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No branches found for company {Id}.", id);
                return NotFound($"No branches found for company with ID {id}.");
            }
            return PartialView("_Branches", response.Data);
        }

        public IActionResult Create()
        {
            return View(new CompanyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
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
                var response = await companyRequest.SaveAsync(model);

                if (!response.Success)
                {
                    logger.LogError("Failed to save company: {Message}", response.Message);
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving company");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid company ID: {Id}", id);
                return BadRequest("Invalid company ID.");
            }
            var response = await companyRequest.SearchAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to find company with ID {Id}: {Message}", id, response.Message);
                return NotFound(response);
            }
            if (response.Data == null)
            {
                logger.LogWarning("Company with ID {Id} not found.", id);
                return NotFound($"Company with ID {id} not found.");
            }
            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyViewModel model)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid company ID: {Id}", id);
                return BadRequest("Invalid company ID.");
            }
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
                var response = await companyRequest.SaveAsync(model);

                if (!response.Success)
                {
                    logger.LogError("Failed to save company: {Message}", response.Message);
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving company");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid company ID: {Id}", id);
                return BadRequest("Invalid company ID.");
            }
            try
            {
                var response = await companyRequest.DeleteAsync(id);
                if (!response.Success)
                {
                    logger.LogError("Failed to delete company with ID {Id}: {Message}", id, response.Message);
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting company with ID {Id}", id);
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.Message}"));
            }
        }
    }
}
