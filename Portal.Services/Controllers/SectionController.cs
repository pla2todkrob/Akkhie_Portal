// FileName: Portal.Services/Controllers/SectionController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController(ISectionService sectionService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await sectionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await sectionService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SectionViewModel viewModel)
        {
            var result = await sectionService.CreateAsync(viewModel);
            if (!result.Success || result.Data == null) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SectionViewModel viewModel)
        {
            var result = await sectionService.UpdateAsync(id, viewModel);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await sectionService.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}