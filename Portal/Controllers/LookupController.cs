using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Interfaces;
using Portal.Models;
using Portal.Shared.Enums.Support;

namespace Portal.Controllers
{
    public class LookupController(
        ILogger<LookupController> logger, 
        IRoleRequest roleRequest, 
        ICompanyRequest companyRequest, 
        IDivisionRequest divisionRequest, 
        IDepartmentRequest departmentRequest, 
        ISectionRequest sectionRequest, 
        ISupportTicketRequest supportTicketRequest,
        IITInventoryRequest itInventoryRequest) : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> GetRoles()
        {
            var response = await roleRequest.GetAllAsync();
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
        [HttpGet]
        public async Task<IActionResult> GetSupportCategories()
        {
            // For now, we fetch only "Issue" type categories for the form.
            var response = await supportTicketRequest.GetCategoriesAsync(TicketCategoryType.Issue.ToString());

            if (!response.Success || response.Data == null)
            {
                return Json(new { success = false, message = response.Message ?? "Could not load categories." });
            }

            var categories = response.Data.Select(s => new {
                id = s.Id,
                name = s.Name
            });

            return Json(new { success = true, data = categories });
        }
        [HttpGet]
        public async Task<IActionResult> GetMySupportTickets()
        {
            var response = await supportTicketRequest.GetMyTicketsAsync();
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketDetails(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid Ticket ID." });
            }
            var response = await supportTicketRequest.GetTicketDetailsAsync(id);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetITStockItems()
        {
            var response = await itInventoryRequest.GetAvailableStockItemsAsync();
            return Json(response);
        }
    }
}
