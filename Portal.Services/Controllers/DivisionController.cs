// FileName: Portal.Services/Controllers/DivisionController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DivisionController : ControllerBase
    {
        private readonly IDivisionService _divisionService;

        public DivisionController(IDivisionService divisionService)
        {
            _divisionService = divisionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _divisionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _divisionService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DivisionViewModel viewModel)
        {
            var result = await _divisionService.CreateAsync(viewModel);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DivisionViewModel viewModel)
        {
            var result = await _divisionService.UpdateAsync(id, viewModel);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _divisionService.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}