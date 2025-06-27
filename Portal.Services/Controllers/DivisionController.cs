using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Services.Models;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DivisionController(IDivisionService divisionService, IDepartmentService departmentService, ILogger<DivisionController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllDivisions()
        {
            try
            {
                var divisions = await divisionService.AllAsync();
                return Ok(ApiResponse.SuccessResponse(divisions, "Divisions retrieved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetAllDivisions));
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpGet("{divisionId}/departments")]
        public async Task<IActionResult> GetDepartmentsByDivision(int divisionId)
        {
            try
            {
                var departments = await departmentService.SearchByDivisionAsync(divisionId);
                if (departments == null || departments.Count == 0)
                {
                    return NotFound(ApiResponse.ErrorResponse("No departments found for the specified division."));
                }
                return Ok(ApiResponse.SuccessResponse(departments, "Departments retrieved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetDepartmentsByDivision));
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDivisionById(int id)
        {
            try
            {
                var division = await divisionService.SearchAsync(id);
                if (division == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Division not found."));
                }
                return Ok(ApiResponse.SuccessResponse(division, "Division retrieved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetDivisionById));
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveDivision([FromBody] DivisionViewModel division)
        {
            if (division == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid division data."));
            }
            try
            {
                var savedDivision = await divisionService.SaveAsync(division);
                return Ok(ApiResponse.SuccessResponse(savedDivision, "Division saved successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(SaveDivision));
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }

        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> DeleteDivision(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid division ID."));
            }
            try
            {
                var isDeleted = await divisionService.DeleteAsync(id);
                if (!isDeleted)
                {
                    return NotFound(ApiResponse.ErrorResponse("Division not found or could not be deleted."));
                }
                return Ok(ApiResponse.SuccessResponse(isDeleted, "Division deleted successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(DeleteDivision));
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}")
                );
            }
        }
    }
}
