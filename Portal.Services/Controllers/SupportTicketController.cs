using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Enums.Support;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.DTOs.Support;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportTicketController(ISupportTicketService supportTicketService, ILogger<SupportTicketController> logger) : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService = supportTicketService;
        private readonly ILogger<SupportTicketController> _logger = logger;


        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("ข้อมูลที่ส่งมาไม่ถูกต้อง",
                    [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]));
            }

            try
            {
                var createdTicket = await _supportTicketService.CreateTicketAsync(request);

                // Return a 201 Created status with the location of the new resource and the resource itself.
                return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, ApiResponse<object>.SuccessResponse(createdTicket, "สร้าง Ticket สำเร็จ"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to create a ticket.");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a support ticket.");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาดในการสร้าง Ticket: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            try
            {
                var ticket = await _supportTicketService.GetTicketDetailsAsync(id);
                if (ticket == null)
                {
                    return NotFound("ไม่พบ Ticket ที่ระบุ");
                }
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket with ID {TicketId}", id);
                return StatusCode(500, $"เกิดข้อผิดพลาดในการดึงข้อมูล Ticket: {ex.GetBaseException().Message}");
            }
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptTicket([FromBody] TicketActionRequest request)
        {
            try
            {
                var result = await _supportTicketService.AcceptTicketAsync(request);
                return result ? Ok(ApiResponse.SuccessResponse(true, "รับงานสำเร็จ"))
                              : BadRequest(ApiResponse.ErrorResponse("ไม่สามารถรับงานได้"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting ticket {TicketId}", request.TicketId);
                return StatusCode(500, ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("resolve")]
        public async Task<IActionResult> ResolveTicket([FromBody] TicketActionRequest request)
        {
            try
            {
                var result = await _supportTicketService.ResolveTicketAsync(request);
                return result ? Ok(ApiResponse.SuccessResponse(true, "บันทึกการแก้ไขสำเร็จ"))
                              : BadRequest(ApiResponse.ErrorResponse("ไม่สามารถบันทึกการแก้ไขได้"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving ticket {TicketId}", request.TicketId);
                return StatusCode(500, ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseTicket([FromBody] TicketActionRequest request)
        {
            try
            {
                var result = await _supportTicketService.CloseTicketByUserAsync(request);
                return Ok(ApiResponse.SuccessResponse(result, "ปิด Ticket เรียบร้อยแล้ว"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to close ticket {TicketId}", request.TicketId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing ticket {TicketId}", request.TicketId);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTicket([FromBody] TicketActionRequest request)
        {
            try
            {
                var result = await _supportTicketService.CancelTicketAsync(request);
                return Ok(ApiResponse.SuccessResponse(result, "ยกเลิก Ticket เรียบร้อยแล้ว"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to cancel ticket {TicketId}", request.TicketId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", request.TicketId);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectTicket([FromBody] TicketActionRequest request)
        {
            try
            {
                var result = await _supportTicketService.RejectTicketAsync(request);
                return Ok(ApiResponse.SuccessResponse(result, "ปฏิเสธ Ticket เรียบร้อยแล้ว"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting ticket {TicketId}", request.TicketId);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("mytickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var tickets = await _supportTicketService.GetMyTicketsAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user's tickets.");
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูล Ticket ของคุณ");
            }
        }

        [HttpGet("myclosedtickets")]
        public async Task<IActionResult> GetMyClosedTickets()
        {
            var tickets = await _supportTicketService.GetMyClosedTicketsAsync();
            return Ok(tickets);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] TicketCategoryType categoryType)
        {
            try
            {
                var categories = await _supportTicketService.GetCategoriesAsync(categoryType);
                if (categories == null || !categories.Any())
                {
                    return NotFound($"ไม่พบหมวดหมู่สำหรับประเภท {categoryType}");
                }
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support categories for type {CategoryType}", categoryType);
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูลหมวดหมู่");
            }
        }
        [HttpPost("withdrawal")]
        public async Task<IActionResult> CreateWithdrawalTicket([FromBody] CreateWithdrawalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("ข้อมูลที่ส่งมาไม่ถูกต้อง",
                    [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]));
            }

            try
            {
                var createdTicket = await _supportTicketService.CreateWithdrawalTicketAsync(request);
                return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, ApiResponse.SuccessResponse(createdTicket, "คำขอเบิกอุปกรณ์ถูกสร้างเรียบร้อยแล้ว"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create withdrawal ticket due to business rule violation.");
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal ticket.");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาดที่ไม่คาดคิด: {ex.Message}"));
            }
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> CreatePurchaseRequestTicket([FromBody] CreatePurchaseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("ข้อมูลที่ส่งมาไม่ถูกต้อง",
                    [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]));
            }

            try
            {
                var createdTicket = await _supportTicketService.CreatePurchaseRequestTicketAsync(request);
                return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, ApiResponse.SuccessResponse(createdTicket, "คำขอจัดซื้อถูกสร้างเรียบร้อยแล้ว"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create purchase request ticket.");
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase request ticket.");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาดที่ไม่คาดคิด: {ex.Message}"));
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTickets()
        {
            try
            {
                var tickets = await _supportTicketService.GetAllTicketsAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all support tickets.");
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูล Ticket ทั้งหมด");
            }
        }
    }
}
