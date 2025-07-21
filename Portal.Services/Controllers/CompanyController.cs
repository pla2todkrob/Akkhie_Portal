// FileName: Portal.Services/Controllers/CompanyController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(ICompanyService companyService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await companyService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await companyService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/branches")]
        public async Task<IActionResult> GetBranchesByCompanyId(int id)
        {
            var result = await companyService.GetBranchesByCompanyIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/divisions")]
        public async Task<IActionResult> GetDivisionsByCompanyId(int id)
        {
            var result = await companyService.GetDivisionsByCompanyIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyViewModel viewModel)
        {
            var result = await companyService.CreateAsync(viewModel);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CompanyViewModel viewModel)
        {
            var result = await companyService.UpdateAsync(id, viewModel);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await companyService.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}