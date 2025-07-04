using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Support;

namespace Portal.Controllers
{
    [Authorize]
    public class SupportController(ISupportTicketRequest supportTicketRequest, ILogger<SupportController> logger) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "จัดการ Support Ticket";
            var response = await supportTicketRequest.GetAllTicketsAsync();
            if (!response.Success)
            {
                // Handle error appropriately, maybe show an error page
                TempData["ErrorMessage"] = response.Message;
                return View(new List<Portal.Shared.Models.ViewModel.Support.TicketListViewModel>());
            }
            return View(response.Data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง", errors });
            }

            try
            {
                var response = await supportTicketRequest.CreateTicketAsync(model);
                return Json(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating support ticket from Portal.");
                return Json(new { success = false, message = $"เกิดข้อผิดพลาด: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithdrawal([FromBody] CreateWithdrawalRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง", errors });
            }

            try
            {
                var response = await supportTicketRequest.CreateWithdrawalTicketAsync(model);
                return Json(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating withdrawal ticket from Portal.");
                return Json(new { success = false, message = $"เกิดข้อผิดพลาด: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง", errors });
            }

            try
            {
                var response = await supportTicketRequest.CreatePurchaseRequestTicketAsync(model);
                return Json(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating purchase request ticket from Portal.");
                return Json(new { success = false, message = $"เกิดข้อผิดพลาด: {ex.Message}" });
            }
        }
    }
}