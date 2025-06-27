using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController(ILogger<DepartmentController> logger, IDepartmentService departmentService, ISectionService sectionService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await departmentService.AllAsync();
                if (departments == null || departments.Count == 0)
                {
                    return Ok(ApiResponse<IEnumerable<DepartmentViewModel>>.ErrorResponse("No departments found."));
                }
                return Ok(ApiResponse<IEnumerable<DepartmentViewModel>>.SuccessResponse(departments));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetAllDepartments));
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while processing your request."));
            }
        }

        [HttpGet("{departmentId}/sections")]
        public async Task<IActionResult> GetSectionsByDepartment(int departmentId)
        {
            try
            {
                var sections = await sectionService.SearchByDepartmentAsync(departmentId);

                if (sections == null || sections.Count == 0)
                {
                    return Ok(ApiResponse<IEnumerable<SectionViewModel>>.ErrorResponse("No sections found for the specified department."));
                }

                return Ok(ApiResponse<IEnumerable<SectionViewModel>>.SuccessResponse(sections));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetSectionsByDepartment));
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while processing your request."));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var department = await departmentService.SearchAsync(id);
                if (department == null)
                {
                    return NotFound(ApiResponse<DepartmentViewModel>.ErrorResponse("Department not found."));
                }
                return Ok(ApiResponse<DepartmentViewModel>.SuccessResponse(department));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetDepartmentById));
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while processing your request."));
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveDepartment([FromBody] DepartmentViewModel model)
        {
            if (model == null)
            {
                return BadRequest(ApiResponse<DepartmentViewModel>.ErrorResponse("Invalid department data."));
            }
            try
            {
                var savedDepartment = await departmentService.SaveAsync(model);
                return Ok(ApiResponse<DepartmentViewModel>.SuccessResponse(savedDepartment, "Department saved successfully."));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(SaveDepartment));
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }

        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var result = await departmentService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Department not found or could not be deleted."));
                }
                return Ok(ApiResponse.SuccessResponse(result, "Department deleted successfully."));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(DeleteDepartment));
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }
    }
}
