using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(ICompanyService companyService, ILogger<CompanyController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                logger.LogInformation("Fetching all companies");
                var companies = await companyService.AllAsync();
                return Ok(ApiResponse.SuccessResponse(companies));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetAllCompanies));
                return StatusCode(500, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var company = await companyService.SearchAsync(id);
                if (company == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Company not found"));
                }
                return Ok(ApiResponse.SuccessResponse(company));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetCompanyById));
                return StatusCode(500, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveCompany([FromBody] CompanyViewModel companyViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Invalid company data", [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]));
                }

                var savedCompany = await companyService.SaveAsync(companyViewModel);

                if (companyViewModel.Id == 0)
                {
                    return CreatedAtAction(nameof(GetCompanyById), new { id = savedCompany.Id },
                        ApiResponse.SuccessResponse(savedCompany, "Company created successfully"));
                }
                else
                {
                    return Ok(ApiResponse.SuccessResponse(savedCompany, "Company updated successfully"));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(SaveCompany));
                return StatusCode(500, ApiResponse.ErrorResponse($"An error occurred while saving company: {ex.GetBaseException().Message}"));
            }
        }

        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            try
            {
                var result = await companyService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Company not found"));
                }

                return Ok(ApiResponse.SuccessResponse(true, "Company deleted successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(DeleteCompany));
                return StatusCode(500, ApiResponse.ErrorResponse($"An error occurred while deleting company: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("{id}/branches")]
        public async Task<IActionResult> GetBranchesByCompany(int id)
        {
            try
            {
                var branches = await companyService.SearchBranchesByCompany(id);
                if (branches == null || branches.Count == 0)
                {
                    return NotFound(ApiResponse.ErrorResponse("No branches found for the specified company"));
                }
                return Ok(ApiResponse.SuccessResponse(branches));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {ActionName}", nameof(GetBranchesByCompany));
                return StatusCode(500, ApiResponse.ErrorResponse($"An error occurred: {ex.GetBaseException().Message}"));
            }
        }
    }
}
