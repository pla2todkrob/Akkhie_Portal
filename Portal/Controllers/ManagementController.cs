using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Portal.Controllers
{
    [Authorize(Policy = "CanAccessManagement")]
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            // ตรวจสอบสิทธิ์: เฉพาะผู้ใช้ที่มี IsSystemRole = true เท่านั้นที่สามารถเข้าหน้านี้ได้
            var isSystemRole = (User.FindFirstValue("IsSystemRole") ?? "false")
                                .Equals("true", StringComparison.OrdinalIgnoreCase);

            if (!isSystemRole)
            {
                // ถ้าไม่มีสิทธิ์ ให้ส่งไปหน้า Access Denied
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewData["Title"] = "หน้าการจัดการข้อมูลหลัก";
            return View();
        }
    }
}
