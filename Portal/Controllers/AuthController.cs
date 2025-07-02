using Microsoft.AspNetCore.Mvc;
using Portal.Shared.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Portal.Interfaces;
using Portal.Shared.Enums;
using Portal.Shared.Models.ViewModel;

namespace Portal.Controllers
{
    public class AuthController(ILogger<AuthController> logger, IEmployeeRequest employeeRequest) : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["Title"] = "เข้าสู่ระบบ";
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLocal(returnUrl);

            return View(new LoginRequest { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await employeeRequest.LoginAsync(model);
                if (response?.Success != true || response.Data == null)
                {
                    AddErrorsToModelState(response?.Errors, response?.Message ?? "เกิดข้อผิดพลาดในการเข้าสู่ระบบ");
                    return View(model);
                }

                var loginResponse = response.Data;
                if (loginResponse.IsNewUser)
                {
                    var registerRequest = new RegisterRequest
                    {
                        Username = loginResponse.Username ?? model.Username,
                        IsAdUser = true,
                        ReturnUrl = model.ReturnUrl,
                        FirstName = loginResponse.ActiveDirectoryProperties?.GetValueOrDefault("givenname") ?? string.Empty,
                        LastName = loginResponse.ActiveDirectoryProperties?.GetValueOrDefault("sn") ?? string.Empty,
                        EmployeeCode = loginResponse.ActiveDirectoryProperties?.GetValueOrDefault("employeeid") ?? string.Empty
                    };
                    return RedirectToAction("Register", registerRequest);
                }

                if (!loginResponse.EmployeeId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "ไม่พบข้อมูลพนักงานที่เกี่ยวข้องกับบัญชีนี้");
                    return View(model);
                }

                var employeeResponse = await employeeRequest.SearchAsync(loginResponse.EmployeeId.Value);
                var employee = employeeResponse.Data;

                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "ไม่พบข้อมูลพนักงานที่เกี่ยวข้องกับบัญชีนี้");
                    return View(model);
                }

                if (employee.EmployeeStatus != EmployeeStatus.Active)
                {
                    ModelState.AddModelError(string.Empty, "บัญชีของคุณยังไม่เปิดใช้งาน กรุณาติดต่อผู้ดูแลระบบ");
                    return View(model);
                }

                await SignInUser(employee, loginResponse.Username ?? model.Username, model.RememberMe, loginResponse.Token!);
                return RedirectToLocal(model.ReturnUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการเข้าสู่ระบบ");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register(RegisterRequest model)
        {
            ViewData["Title"] = "ลงทะเบียน";
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLocal(model.ReturnUrl);

            return View(model);
        }

        [ActionName("Register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterData(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await employeeRequest.RegisterAsync(model);
                if (response?.Success != true || response.Data == null)
                {
                    AddErrorsToModelState(response?.Errors, "เกิดข้อผิดพลาดในการลงทะเบียน");
                    return View(model);
                }

                var registerData = response.Data;
                if (!registerData.EmployeeId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "ไม่พบรหัสพนักงานหลังลงทะเบียน");
                    return View(model);
                }

                if (registerData.EmployeeStatus != EmployeeStatus.Active)
                {
                    ModelState.AddModelError(string.Empty, "บัญชีของคุณยังไม่เปิดใช้งาน กรุณาติดต่อผู้ดูแลระบบ");
                    return RedirectToAction("Login", "Auth");
                }

                var employeeResponse = await employeeRequest.SearchAsync(registerData.EmployeeId.Value);
                var employee = employeeResponse.Data;
                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "ไม่พบข้อมูลพนักงาน");
                    return View(model);
                }

                await SignInUser(employee, employee.Username, true, registerData.Token ?? throw new Exception("Token not found"));
                return RedirectToLocal(model.ReturnUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการลงทะเบียน");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private IActionResult RedirectToLocal(string? returnUrl)
            => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");

        private void AddErrorsToModelState(IEnumerable<string>? errors, string defaultMessage)
        {
            if (errors != null)
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            else
            {
                ModelState.AddModelError(string.Empty, defaultMessage);
            }
        }

        private async Task SignInUser(EmployeeViewModel employee, string username, bool isPersistent, string accessToken)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Email, employee.Email ?? string.Empty),
                new(ClaimTypes.Role, employee.RoleName ?? string.Empty),
                new("EmployeeCode", employee.EmployeeCode ?? string.Empty),
                new("IsSystemRole", employee.IsSystemRole.ToString()),
                new("access_token", accessToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }
    }
}
