using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Enums;
using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.DTOs.Shared;
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

        public async Task<ApiResponse> UpdateStatusAsync(Guid employeeId, EmployeeStatus newStatus)
        {
            var employee = await context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return ApiResponse.ErrorResponse("ไม่พบข้อมูลพนักงาน");
            }

            employee.EmployeeStatus = newStatus;
            await context.SaveChangesAsync();

            return ApiResponse.SuccessResponse(true, "อัปเดตสถานะสำเร็จ");
        }

        public async Task<ApiResponse<EmployeeViewModel>> CreateAsync(RegisterRequest request)
        {
            if (await context.Employees.AnyAsync(e => e.Username == request.Username))
                return ApiResponse<EmployeeViewModel>.ErrorResponse("ชื่อผู้ใช้นี้มีอยู่ในระบบแล้ว");

            if (await context.EmployeeDetails.AnyAsync(ed => ed.EmployeeCode == request.EmployeeCode))
                return ApiResponse<EmployeeViewModel>.ErrorResponse("รหัสพนักงานนี้มีอยู่ในระบบแล้ว");

            var newEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = request.Password != null ? BCrypt.Net.BCrypt.HashPassword(request.Password) : null,
                IsAdUser = request.IsAdUser,
                CompanyId = request.CompanyId,
                DivisionId = request.DivisionId,
                DepartmentId = request.DepartmentId,
                SectionId = request.SectionId,
                RoleId = request.RoleId ?? 0,
                IsSystemRole = request.IsSystemRole,
                CreatedAt = DateTime.UtcNow,
                EmployeeStatus = EmployeeStatus.Active,
                EmployeeDetail = new EmployeeDetail
                {
                    EmployeeCode = request.EmployeeCode,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    LocalFirstName = request.LocalFirstName,
                    LocalLastName = request.LocalLastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                }
            };

            context.Employees.Add(newEmployee);
            await context.SaveChangesAsync();

            var createdEmployeeViewModel = await SearchAsync(newEmployee.Id);
            return ApiResponse<EmployeeViewModel>.SuccessResponse(createdEmployeeViewModel, "สร้างข้อมูลพนักงานสำเร็จ");
        }
        public async Task<ApiResponse> UpdateAsync(Guid employeeId, RegisterRequest request)
        {
            var employee = await context.Employees
                .Include(e => e.EmployeeDetail)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return ApiResponse.ErrorResponse("ไม่พบข้อมูลพนักงาน");

            employee.Username = request.Username;
            employee.CompanyId = request.CompanyId;
            employee.DivisionId = request.DivisionId;
            employee.DepartmentId = request.DepartmentId;
            employee.SectionId = request.SectionId;
            employee.RoleId = request.RoleId ?? 0;
            employee.IsSystemRole = request.IsSystemRole;

            if (employee.EmployeeDetail != null)
            {
                employee.EmployeeDetail.EmployeeCode = request.EmployeeCode;
                employee.EmployeeDetail.FirstName = request.FirstName;
                employee.EmployeeDetail.LastName = request.LastName;
                employee.EmployeeDetail.LocalFirstName = request.LocalFirstName;
                employee.EmployeeDetail.LocalLastName = request.LocalLastName;
                employee.EmployeeDetail.Email = request.Email;
                employee.EmployeeDetail.PhoneNumber = request.PhoneNumber;
            }

            await context.SaveChangesAsync();
            return ApiResponse.SuccessResponse(true, "อัปเดตข้อมูลพนักงานสำเร็จ");
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
                CompanyId = employee.CompanyId,
                CompanyName = employee.Company?.Name ?? string.Empty,
                DivisionId = employee.DivisionId ?? 0,
                DivisionName = employee.Division?.Name,
                DepartmentId = employee.DepartmentId ?? 0,
                DepartmentName = employee.Department?.Name,
                SectionId = employee.SectionId ?? 0,
                SectionName = employee.Section?.Name,
                RoleId = employee.RoleId,
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
