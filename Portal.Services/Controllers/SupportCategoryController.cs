using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportCategoryController(ISupportCategoryService categoryService, ILogger<SupportCategoryController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await categoryService.GetAllAsync();
            return Ok(ApiResponse.SuccessResponse(categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found."));
            }
            return Ok(ApiResponse.SuccessResponse(category));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupportCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }
            var createdCategory = await categoryService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, ApiResponse.SuccessResponse(createdCategory));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupportCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("Mismatched ID."));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }
            try
            {
                var updatedCategory = await categoryService.UpdateAsync(model);
                return Ok(ApiResponse.SuccessResponse(updatedCategory));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await categoryService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found."));
            }
            return Ok(ApiResponse.SuccessResponse(true, "Category deleted successfully."));
        }
    }
}
