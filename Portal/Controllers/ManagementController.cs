using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Portal.Controllers
{
    [Authorize]
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "หน้าการจัดการข้อมูลหลัก";
            return View();
        }
    }
}
