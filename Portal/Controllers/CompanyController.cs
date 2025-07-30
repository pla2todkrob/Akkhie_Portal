using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class CompanyController(ICompanyRequest companyRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var companies = await companyRequest.GetAllAsync();
            return View(companies);
        }

        public async Task<IActionResult> Branches(int id)
        {
            var branches = await companyRequest.GetBranchesByCompanyIdAsync(id);
            return PartialView("_Branches", branches);
        }

        public async Task<IActionResult> Divisions(int id)
        {
            var divisions = await companyRequest.GetDivisionsByCompanyIdAsync(id);

            return PartialView("_Divisions", divisions);
        }

        public IActionResult Create()
        {
            var model = new CompanyViewModel
            {
                CompanyBranchViewModels = [new()]
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await companyRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await companyRequest.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await companyRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await companyRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
