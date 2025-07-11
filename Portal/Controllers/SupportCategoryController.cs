using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Controllers
{
    [Authorize]
    public class SupportCategoryController(ISupportCategoryRequest categoryRequest, ILogger<SupportCategoryController> logger) : Controller
    {
        // GET: SupportCategory
        public async Task<IActionResult> Index()
        {
            var response = await categoryRequest.GetAllAsync();
            if (!response.Success)
            {
                // Handle error appropriately, maybe show an error page or a message
                TempData["ErrorMessage"] = response.Message;
                return View(new List<SupportCategoryViewModel>());
            }
            return View(response.Data);
        }

        // GET: SupportCategory/_CreateOrEdit/{id?}
        public async Task<IActionResult> _CreateOrEdit(int id = 0)
        {
            if (id == 0)
            {
                // This is for creating a new category
                return PartialView("_SupportCategoryModal", new SupportCategoryViewModel());
            }
            else
            {
                // This is for editing an existing category
                var response = await categoryRequest.GetByIdAsync(id);
                if (!response.Success || response.Data == null)
                {
                    return NotFound(); // Or handle the error as you see fit
                }
                return PartialView("_SupportCategoryModal", response.Data);
            }
        }

        // POST: SupportCategory/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SupportCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var response = model.Id == 0
                    ? await categoryRequest.CreateAsync(model)
                    : await categoryRequest.UpdateAsync(model);

                if (response.Success)
                {
                    return Json(new { success = true, message = "บันทึกข้อมูลสำเร็จ" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message, errors = response.Errors });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving support category.");
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล" });
            }
        }

        // POST: SupportCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await categoryRequest.DeleteAsync(id);
                if (response.Success)
                {
                    return Json(new { success = true, message = "ลบข้อมูลสำเร็จ" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting support category with ID {Id}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการลบข้อมูล" });
            }
        }
    }
}
