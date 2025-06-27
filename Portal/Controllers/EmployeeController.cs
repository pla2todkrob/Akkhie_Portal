using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
