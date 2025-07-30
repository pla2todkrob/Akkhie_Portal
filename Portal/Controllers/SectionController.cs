using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class SectionController(ISectionRequest sectionRequest, ICompanyRequest companyRequest, IDivisionRequest divisionRequest, IDepartmentRequest departmentRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var sections = await sectionRequest.GetAllAsync();
            return View(sections);
        }
        private async Task PopulateDropdowns(SectionViewModel? model = null)
        {
            var companies = await companyRequest.GetAllAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name", model?.CompanyId);

            var divisions = model?.CompanyId > 0
                ? await companyRequest.GetDivisionsByCompanyIdAsync(model.CompanyId)
                : new List<DivisionViewModel>();
            ViewBag.Divisions = new SelectList(divisions, "Id", "Name", model?.DivisionId);

            var departments = model?.DivisionId > 0
                ? await divisionRequest.GetDepartmentsByDivisionIdAsync(model.DivisionId)
                : new List<DepartmentViewModel>();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", model?.DepartmentId);
        }


        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new SectionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await sectionRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model);
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var section = await sectionRequest.GetByIdAsync(id);
            if (section == null) return NotFound();

            await PopulateDropdowns(section);
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SectionViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var response = await sectionRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
            }
            await PopulateDropdowns(model);
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await sectionRequest.DeleteAsync(id);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
