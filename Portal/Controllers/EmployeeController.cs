using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.ViewModel;
using Portal.Shared.Enums;

namespace Portal.Controllers
{
    public class EmployeeController(
        IEmployeeRequest employeeRequest,
        ICompanyRequest companyRequest,
        IDivisionRequest divisionRequest,
        IDepartmentRequest departmentRequest,
        ISectionRequest sectionRequest,
        IRoleRequest roleRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var response = await employeeRequest.GetAsync();
            var employees = response.Success ? response.Data ?? new List<EmployeeViewModel>() : new List<EmployeeViewModel>();
            return View(employees);
        }

        private async Task PopulateDropdowns(RegisterRequest? model = null)
        {
            var companies = await companyRequest.GetAllAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name", model?.CompanyId);

            var roles = await roleRequest.GetAllAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name", model?.RoleId);

            var divisions = model?.CompanyId > 0
                ? await companyRequest.GetDivisionsByCompanyIdAsync(model.CompanyId)
                : new List<DivisionViewModel>();
            ViewBag.Divisions = new SelectList(divisions, "Id", "Name", model?.DivisionId);

            var departments = model?.DivisionId > 0
                ? await divisionRequest.GetDepartmentsByDivisionIdAsync(model.DivisionId.Value)
                : new List<DepartmentViewModel>();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", model?.DepartmentId);

            var sections = model?.DepartmentId > 0
                ? await departmentRequest.GetSectionsByDepartmentIdAsync(model.DepartmentId.Value)
                : new List<SectionViewModel>();
            ViewBag.Sections = new SelectList(sections, "Id", "Name", model?.SectionId);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View("Form", new RegisterRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterRequest model)
        {
            if (ModelState.IsValid)
            {
                var response = await employeeRequest.CreateAsync(model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
                if (response.Errors != null)
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            await PopulateDropdowns(model);
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await employeeRequest.SearchAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound();
            }
            var employee = response.Data;

            var model = new RegisterRequest
            {
                Id = employee.Id,
                Username = employee.Username,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                LocalFirstName = employee.LocalFirstName,
                LocalLastName = employee.LocalLastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                CompanyId = employee.CompanyId,
                DivisionId = employee.DivisionId,
                DepartmentId = employee.DepartmentId,
                SectionId = employee.SectionId,
                RoleId = employee.RoleId,
                IsSystemRole = employee.IsSystemRole,
                IsAdUser = employee.IsAdUser
            };

            await PopulateDropdowns(model);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RegisterRequest model)
        {
            if (id != model.Id) return BadRequest();

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                var response = await employeeRequest.UpdateAsync(id, model);
                if (response.Success)
                {
                    return Ok(response);
                }
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
                if (response.Errors != null)
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            await PopulateDropdowns(model);
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, EmployeeStatus status)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Invalid Employee ID." });
            }

            var response = await employeeRequest.UpdateStatusAsync(id, status);
            return Json(response);
        }
    }
}
