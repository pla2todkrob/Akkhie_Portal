using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Services.Models;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController(ISectionService sectionService, ILogger<SectionController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllSections()
        {
            try
            {
                var sections = await sectionService.AllAsync();
                if (sections == null || sections.Count == 0)
                {
                    return Ok(ApiResponse<IEnumerable<SectionViewModel>>.ErrorResponse("No sections found."));
                }
                return Ok(ApiResponse<IEnumerable<SectionViewModel>>.SuccessResponse(sections));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting all sections.");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSectionById(int id)
        {
            try
            {
                var section = await sectionService.SearchAsync(id);
                if (section == null)
                {
                    return NotFound(ApiResponse<SectionViewModel>.ErrorResponse("Section not found."));
                }
                return Ok(ApiResponse<SectionViewModel>.SuccessResponse(section));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting section by id {SectionId}.", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveSection([FromBody] SectionViewModel model)
        {
            if (model == null)
            {
                return BadRequest(ApiResponse<SectionViewModel>.ErrorResponse("Invalid section data."));
            }
            try
            {
                var savedSection = await sectionService.SaveAsync(model);
                return Ok(ApiResponse<SectionViewModel>.SuccessResponse(savedSection, "Section saved successfully."));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(SaveSection));
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }


        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                var result = await sectionService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Section not found or could not be deleted."));
                }
                return Ok(ApiResponse.SuccessResponse(result, "Section deleted successfully."));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(DeleteSection));
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }
    }
}