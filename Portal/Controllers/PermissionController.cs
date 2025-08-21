using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Models.Entities;

namespace Portal.Controllers
{
    [Authorize(Policy = "CanAccessManagement")]
    public class PermissionController(IPermissionRequest permissionRequest) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var permissions = await permissionRequest.GetAllAsync();
            return View(permissions);
        }

        public async Task<IActionResult> _CreateOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return PartialView(new Permission());
            }
            var model = await permissionRequest.GetByIdAsync(id);
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Permission model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            if (model.Id == 0)
            {
                var response = await permissionRequest.CreateAsync(model);
                return Json(response);
            }
            else
            {
                var response = await permissionRequest.UpdateAsync(model.Id, model);
                return Json(response);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await permissionRequest.DeleteAsync(id);
            return Json(response);
        }
    }
}