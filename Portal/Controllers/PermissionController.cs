using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.Entities;

namespace Portal.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IPermissionRequest _permissionRequest;

        public PermissionController(IPermissionRequest permissionRequest)
        {
            _permissionRequest = permissionRequest;
        }

        // GET: Permission
        public async Task<IActionResult> Index()
        {
            var response = await _permissionRequest.GetAllAsync();
            var permissions = response.Success ? response.Data ?? new List<Permission>() : new List<Permission>();
            return View(permissions);
        }

        // GET: Permission/Create
        public IActionResult Create()
        {
            return View("Form", new Permission());
        }

        // POST: Permission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Permission permission)
        {
            if (ModelState.IsValid)
            {
                var response = await _permissionRequest.CreateAsync(permission);
                if (response.Success)
                {
                    return Ok(response);
                }
                // Add errors to ModelState to return to the form
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
                if (response.Errors != null)
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            return BadRequest(ModelState);
        }

        // GET: Permission/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _permissionRequest.GetByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound();
            }
            return View("Form", response.Data);
        }

        // POST: Permission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var response = await _permissionRequest.UpdateAsync(id, permission);
                if (response.Success)
                {
                    return Ok(response);
                }
                // Add errors to ModelState to return to the form
                ModelState.AddModelError(string.Empty, response.Message ?? "An unknown error occurred.");
                if (response.Errors != null)
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            return BadRequest(ModelState);
        }

        // DELETE: Permission/Delete/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _permissionRequest.DeleteAsync(id);
            return Json(response);
        }
    }
}
