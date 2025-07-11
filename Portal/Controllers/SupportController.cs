using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Controllers
{
    [Authorize]
    public class SupportController(
        ISupportTicketRequest supportTicketRequest,
        IEmployeeRequest employeeRequest,
        ISupportCategoryRequest categoryRequest,
        ILogger<SupportController> logger) : Controller
    {
        // GET: Support/Index
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "จัดการ Support Ticket";
            var response = await supportTicketRequest.GetAllTicketsAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new List<TicketListViewModel>());
            }
            return View(response.Data);
        }

        // POST: Support/Create
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


        // GET: Support/Details/5
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "รายละเอียด Ticket";
            var response = await supportTicketRequest.GetTicketDetailsAsync(id);

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "ไม่พบข้อมูล Ticket ที่ระบุ";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();

            return View(response.Data);
        }

        // POST: Support/Accept
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(TicketActionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }
            var response = await supportTicketRequest.AcceptTicketAsync(request);
            return Json(response);
        }

        // POST: Support/Resolve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(TicketActionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }
            var response = await supportTicketRequest.ResolveTicketAsync(request);
            return Json(response);
        }

        // POST: Support/CreateWithdrawal
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

        // POST: Support/CreatePurchase
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


        // --- Helper Method ---
        private async Task PopulateDropdowns()
        {
            // ดึงข้อมูลพนักงานแผนก IT (Section "สารสนเทศ" Id = 2)
            var employeesResponse = await employeeRequest.GetAsync(sectionId: 2);
            if (employeesResponse.Success && employeesResponse.Data != null)
            {
                ViewBag.ITUsers = employeesResponse.Data
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FullName })
                    .ToList();
            }

            // ดึงข้อมูลหมวดหมู่ทั้งหมด
            var categoriesResponse = await categoryRequest.GetAllAsync();
            if (categoriesResponse.Success && categoriesResponse.Data != null)
            {
                ViewBag.Categories = categoriesResponse.Data
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();
            }

            // สร้าง SelectList สำหรับ Priority
            ViewBag.Priorities = new SelectList(Enum.GetValues<TicketPriority>()
                .Cast<TicketPriority>()
                .Select(p => new { Value = (int)p, Text = p.GetDisplayName() }), "Value", "Text");
        }
    }
}
