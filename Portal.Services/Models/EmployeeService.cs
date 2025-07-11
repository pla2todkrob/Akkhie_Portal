using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Models
{
    public class EmployeeService(PortalDbContext context, IHttpContextAccessor httpContextAccessor) : IEmployeeService
    {
        public async Task<EmployeeViewModel?> SearchAsync(Guid employeeId)
        {
            var employee = await _employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            return employee == null ? null : SetViewModel(employee);
        }

        public async Task<EmployeeViewModel?> SearchByUsernameAsync(string username)
        {
            var employee = await _employees.FirstOrDefaultAsync(e => e.Username == username);
            return employee == null ? null : SetViewModel(employee);
        }

        public async Task<EmployeeViewModel?> SearchByEmailAsync(string email)
        {
            var employee = await _employees.FirstOrDefaultAsync(e => e.EmployeeDetail!.Email == email);
            return employee == null ? null : SetViewModel(employee);
        }

        public async Task<List<EmployeeViewModel>> GetAsync(int? companyId = null, int? divisionId = null, int? departmentId = null, int? sectionId = null)
        {
            var query = _employees;

            if (companyId.HasValue)
            {
                query = query.Where(e => e.EmployeeCompanyAccesses.Any(eca => eca.CompanyId == companyId.Value));
            }
            if (divisionId.HasValue)
            {
                query = query.Where(e => e.DivisionId == divisionId.Value);
            }
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }
            if (sectionId.HasValue)
            {
                query = query.Where(e => e.SectionId == sectionId.Value);
            }

            var employees = await query.ToListAsync();
            return [.. employees.Select(SetViewModel)];
        }

        private EmployeeViewModel SetViewModel(Employee employee)
        {
            return new EmployeeViewModel
            {
                Id = employee.Id,
                Username = employee.Username,
                IsAdUser = employee.IsAdUser,
                IsSystemRole = employee.IsSystemRole,
                PhoneNumber = employee.EmployeeDetail!.PhoneNumber,
                CompanyName = employee.Company?.Name ?? string.Empty,
                DivisionName = employee.Division?.Name,
                DepartmentName = employee.Department?.Name,
                SectionName = employee.Section?.Name,
                RoleName = employee.Role.Name,
                CreatedAt = employee.CreatedAt,
                EmployeeStatus = employee.EmployeeStatus,
                EmployeeCode = employee.EmployeeDetail.EmployeeCode,
                FirstName = employee.EmployeeDetail.FirstName,
                LastName = employee.EmployeeDetail.LastName,
                Email = employee.EmployeeDetail.Email,
                FullName = employee.EmployeeDetail.FullName,
                LocalFirstName = employee.EmployeeDetail.LocalFirstName,
                LocalLastName = employee.EmployeeDetail.LocalLastName,
                LocalFullName = employee.EmployeeDetail.LocalFullName,
                ProfileUrl = GetProfileUrl(employee)
            };
        }

        private string GetProfileUrl(Employee employee)
        {
            if (employee.ProfilePicture == null || string.IsNullOrEmpty(employee.ProfilePicture.UploadPath))
                return string.Empty;

            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            var uploadPath = employee.ProfilePicture.UploadPath.StartsWith('/')
                ? employee.ProfilePicture.UploadPath
                : "/" + employee.ProfilePicture.UploadPath;

            return baseUrl + uploadPath;
        }

        private readonly IQueryable<Employee> _employees = context.Employees
            .Include(i => i.ProfilePicture)
            .Include(i => i.Company)
            .Include(i => i.Division)
            .Include(i => i.Department)
            .Include(i => i.Section)
            .Include(i => i.Role)
            .Include(i => i.EmployeeDetail)
            .Include(i => i.EmployeeCompanyAccesses);
    }
}
