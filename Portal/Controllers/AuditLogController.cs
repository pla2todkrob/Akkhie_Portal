using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class AuditLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
