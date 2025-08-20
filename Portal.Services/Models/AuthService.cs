using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.Entities;
using Portal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Portal.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Portal.Services.Models
{
    public class AuthService(
        PortalDbContext context,
        IActiveDirectoryService adService,
        IOptions<JwtSettings> jwtSettingsOptions,
        IAuditLogService auditLogService) : IAuthService
    {
        private readonly PortalDbContext _context = context;
        private readonly IActiveDirectoryService _adService = adService;
        private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;
        private readonly IAuditLogService _auditLogService = auditLogService;

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            var employees = await _context.Employees
                .Include(i => i.Role)
                .Include(i => i.EmployeeDetail)
                .Where(e => e.Username == request.Username)
                .ToListAsync();

            if (employees.Any())
            {
                Employee? validEmployee = null;

                foreach (var employee in employees)
                {
                    if (employee.EmployeeStatus != EmployeeStatus.Active)
                    {
                        continue;
                    }

                    bool isValid;
                    if (employee.IsAdUser)
                    {
                        isValid = await _adService.ValidateCredentials(request.Username, request.Password);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(employee.PasswordHash)) continue;
                        isValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
                    }

                    if (isValid)
                    {
                        validEmployee = employee;
                        break;
                    }
                }

                if (validEmployee == null)
                {
                    var hasActiveUser = employees.Any(e => e.EmployeeStatus == EmployeeStatus.Active);
                    if (!hasActiveUser)
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            ErrorMessage = $"บัญชีผู้ใช้นี้ทั้งหมดไม่ Active ไม่สามารถเข้าสู่ระบบได้"
                        };
                    }

                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง"
                    };
                }

                var token = GenerateJwtToken(validEmployee);
                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    EmployeeId = validEmployee.Id,
                    Username = validEmployee.Username
                };
            }
            else
            {
                var isValidAd = await _adService.ValidateCredentials(request.Username, request.Password);
                if (!isValidAd)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง"
                    };
                }

                var adProps = await _adService.GetUserProperties(request.Username, ["givenname", "sn", "employeeid"]);

                return new LoginResponse
                {
                    Success = true,
                    Username = request.Username,
                    IsNewUser = true,
                    ActiveDirectoryProperties = adProps
                };
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, string? ipAddress)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = [.. validationResults.Select(r => r.ErrorMessage ?? string.Empty)]
                };
            }

            if (await _context.Employees.AnyAsync(e => e.Username == request.Username && e.CompanyId == request.CompanyId))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["ชื่อผู้ใช้นี้มีอยู่ในบริษัทนี้แล้ว"]
                };
            }

            var isEmployeeCodeExists = await _context.EmployeeDetails
                .AnyAsync(ed => ed.EmployeeCode == request.EmployeeCode && ed.Employee.CompanyId == request.CompanyId);

            if (isEmployeeCodeExists)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["รหัสพนักงานนี้มีอยู่ในบริษัทนี้แล้ว"]
                };
            }

            string? passwordHash = null;
            if (!request.IsAdUser)
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password!);
            }

            var newEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = passwordHash,
                IsAdUser = request.IsAdUser,
                CompanyId = request.CompanyId,
                DivisionId = request.DivisionId,
                DepartmentId = request.DepartmentId,
                SectionId = request.SectionId,
                CreatedAt = DateTime.UtcNow,
                RoleId = request.RoleId ?? (int)RoleType.Staff,
                IsSystemRole = !_context.Employees.Any() || request.IsSystemRole,
                EmployeeStatus = (request.DivisionId.HasValue && request.DepartmentId.HasValue && request.SectionId.HasValue)
                                 ? EmployeeStatus.Active
                                 : EmployeeStatus.Inactive
            };

            var newEmployeeDetail = new EmployeeDetail
            {
                EmployeeId = newEmployee.Id,
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                LocalFirstName = request.LocalFirstName,
                LocalLastName = request.LocalLastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            };

            newEmployee.EmployeeDetail = newEmployeeDetail;

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId);
            if (!companyExists)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = [$"ไม่พบ Company Id: {request.CompanyId}"]
                };
            }

            var employeeCompanyAccess = new EmployeeCompanyAccess
            {
                EmployeeId = newEmployee.Id,
                CompanyId = request.CompanyId,
                AccessLevel = newEmployee.IsSystemRole ? AccessLevel.Admin : AccessLevel.Read,
                GrantedDate = DateTime.UtcNow
            };

            if (request.CompanyBranchId.HasValue)
            {
                var branchExists = await _context.CompanyBranches
                    .AnyAsync(cb => cb.Id == request.CompanyBranchId.Value && cb.CompanyId == request.CompanyId);
                if (!branchExists)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Errors = [$"ไม่พบ CompanyBranch Id: {request.CompanyBranchId.Value} สำหรับ Company Id: {request.CompanyId}"]
                    };
                }
                employeeCompanyAccess.CompanyBranchId = request.CompanyBranchId.Value;
            }

            _context.EmployeeCompanyAccesses.Add(employeeCompanyAccess);

            _context.Employees.Add(newEmployee);

            try
            {
                await _context.SaveChangesAsync();
                var token = GenerateJwtToken(newEmployee);
                return new RegisterResponse
                {
                    Success = true,
                    EmployeeId = newEmployee.Id,
                    EmployeeStatus = newEmployee.EmployeeStatus,
                    Token = token
                };
            }
            catch (DbUpdateException ex)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["เกิดข้อผิดพลาดในการบันทึกข้อมูล", ex.Message]
                };
            }
        }

        private string GenerateJwtToken(Employee employee)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new(ClaimTypes.Name, employee.Username),
                new(JwtRegisteredClaimNames.Sub, employee.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("IsAdUser", employee.IsAdUser.ToString())
            };

            if (employee.EmployeeDetail != null)
            {
                claims.Add(new Claim("EmployeeCode", employee.EmployeeDetail.EmployeeCode));
                claims.Add(new Claim(ClaimTypes.GivenName, employee.EmployeeDetail.FirstName));
                claims.Add(new Claim(ClaimTypes.Surname, employee.EmployeeDetail.LastName));
                claims.Add(new Claim(ClaimTypes.Email, employee.EmployeeDetail.Email));
                claims.Add(new Claim("FullName", $"{employee.EmployeeDetail.FirstName} {employee.EmployeeDetail.LastName}"));
                if (!string.IsNullOrEmpty(employee.EmployeeDetail.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, employee.EmployeeDetail.PhoneNumber));
                }
            }

            claims.Add(new Claim(ClaimTypes.Role, employee.Role?.Name ?? "Staff"));
            claims.Add(new Claim("RoleId", employee.RoleId.ToString()));
            claims.Add(new Claim("EmployeeStatus", employee.EmployeeStatus.ToString()));

            if (employee.DivisionId.HasValue) claims.Add(new Claim("DivisionId", employee.DivisionId.Value.ToString()));
            if (employee.DepartmentId.HasValue) claims.Add(new Claim("DepartmentId", employee.DepartmentId.Value.ToString()));
            if (employee.SectionId.HasValue) claims.Add(new Claim("SectionId", employee.SectionId.Value.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
