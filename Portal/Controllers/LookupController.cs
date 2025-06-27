using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    public class LookupController(ILogger<LookupController> logger, IRoleRequest roleRequest, ICompanyRequest companyRequest, IDivisionRequest divisionRequest, IDepartmentRequest departmentRequest, ISectionRequest sectionRequest) : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> GetRoles()
        {
            var response = await roleRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get roles: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No roles found.");
                return NotFound("No roles found.");
            }

            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }

        public async Task<IActionResult> GetCompanies()
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
            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }

        public async Task<IActionResult> GetBranchesByCompany(int id)
        {
            var response = await companyRequest.GetBranchesByCompanyAsync(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get branches for company {CompanyId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No branches found for company {CompanyId}.", id);
                return NotFound("No branches found for the specified company.");
            }
            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }

        public async Task<IActionResult> GetDivisions()
        {
            var response = await divisionRequest.AllAsync();
            if (!response.Success)
            {
                logger.LogWarning("Failed to get divisions: {Message}", response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No divisions found.");
                return NotFound("No divisions found.");
            }
            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }

        public async Task<IActionResult> GetDepartmentsByDivision(int id)
        {
            var response = await departmentRequest.SearchByDivision(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get departments for division {DivisionId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No departments found for division {DivisionId}.", id);
                return NotFound("No departments found for the specified division.");
            }
            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }

        public async Task<IActionResult> GetSectionsByDepartment(int id)
        {
            var response = await sectionRequest.SearchByDepartment(id);
            if (!response.Success)
            {
                logger.LogWarning("Failed to get sections for department {DepartmentId}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            if (response.Data == null || !response.Data.Any())
            {
                logger.LogWarning("No sections found for department {DepartmentId}.", id);
                return NotFound("No sections found for the specified department.");
            }
            return Json(response.Data.Select(s => new
            {
                value = s.Id.ToString(),
                text = s.Name
            }));
        }
    }
}
