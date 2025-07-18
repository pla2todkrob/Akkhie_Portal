// FileName: Portal.Services/Controllers/CompanyController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _companyService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/branches")]
        public async Task<IActionResult> GetBranchesByCompanyId(int id)
        {
            var result = await _companyService.GetBranchesByCompanyIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/divisions")]
        public async Task<IActionResult> GetDivisionsByCompanyId(int id)
        {
            var result = await _companyService.GetDivisionsByCompanyIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyViewModel viewModel)
        {
            var result = await _companyService.CreateAsync(viewModel);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CompanyViewModel viewModel)
        {
            var result = await _companyService.UpdateAsync(id, viewModel);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _companyService.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}