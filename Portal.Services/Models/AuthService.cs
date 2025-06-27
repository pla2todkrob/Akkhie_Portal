// Portal.Services/Models/AuthService.cs
using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.Entities;
using Portal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using Portal.Shared.Enums;
using Portal.Shared.Models.DTOs.Shared;
using System.Linq;
using Portal.Shared.Constants;
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
            var employee = await _context.Employees
                .Include(i => i.Role)
                .Include(i => i.EmployeeDetail)
                .FirstOrDefaultAsync(e => e.Username == request.Username);

            if (employee != null)
            {
                if (employee.EmployeeStatus != EmployeeStatus.Active)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = $"บัญชีผู้ใช้นี้ {employee.EmployeeStatus.GetDisplayName()} ไม่สามารถเข้าสู่ระบบได้"
                    };
                }

                bool isValid;
                if (employee.IsAdUser)
                {
                    isValid = await _adService.ValidateCredentials(request.Username, request.Password);
                }
                else
                {
                    isValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
                }

                if (!isValid)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง"
                    };
                }

                var token = GenerateJwtToken(employee);
                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    EmployeeId = employee.Id,
                    Username = employee.Username
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

            if (await _context.Employees.AnyAsync(e => e.Username == request.Username))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["ชื่อผู้ใช้นี้มีอยู่ในระบบแล้ว"]
                };
            }

            if (await _context.EmployeeDetails.AnyAsync(ed => ed.Email == request.Email))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["อีเมลนี้มีอยู่ในระบบแล้ว"]
                };
            }

            if (await _context.EmployeeDetails.AnyAsync(ed => ed.EmployeeCode == request.EmployeeCode))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = ["รหัสพนักงานนี้มีอยู่ในระบบแล้ว"]
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

            if (request.CompanyId.HasValue)
            {
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId.Value);
                if (!companyExists)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Errors = [$"ไม่พบ Company Id: {request.CompanyId.Value}"]
                    };
                }

                var employeeCompanyAccess = new EmployeeCompanyAccess
                {
                    EmployeeId = newEmployee.Id,
                    CompanyId = request.CompanyId.Value,
                    AccessLevel = AccessLevel.Read,
                    GrantedDate = DateTime.UtcNow
                };

                if (request.CompanyBranchId.HasValue)
                {
                    var branchExists = await _context.CompanyBranches
                        .AnyAsync(cb => cb.Id == request.CompanyBranchId.Value && cb.CompanyId == request.CompanyId.Value);
                    if (!branchExists)
                    {
                        return new RegisterResponse
                        {
                            Success = false,
                            Errors = [$"ไม่พบ CompanyBranch Id: {request.CompanyBranchId.Value} สำหรับ Company Id: {request.CompanyId.Value}"]
                        };
                    }
                    employeeCompanyAccess.CompanyBranchId = request.CompanyBranchId.Value;
                }
                _context.EmployeeCompanyAccesses.Add(employeeCompanyAccess);
            }

            _context.Employees.Add(newEmployee);

            try
            {
                await _context.SaveChangesAsync();

                return new RegisterResponse
                {
                    Success = true,
                    EmployeeId = newEmployee.Id,
                    EmployeeStatus = newEmployee.EmployeeStatus
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
