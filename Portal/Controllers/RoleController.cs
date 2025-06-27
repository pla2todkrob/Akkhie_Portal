using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class RoleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
