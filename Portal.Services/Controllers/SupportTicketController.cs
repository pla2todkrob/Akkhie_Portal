using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
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

        /// <summary>
        /// Creates a new support ticket.
        /// </summary>
        /// <param name="request">The data for the new ticket.</param>
        /// <returns>The created ticket.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
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
                return CreatedAtAction(nameof(GetTicket), new { id = createdTicket.Id }, ApiResponse<object>.SuccessResponse(createdTicket, "สร้าง Ticket สำเร็จ"));
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

        /// <summary>
        /// Gets a specific support ticket by its ID.
        /// </summary>
        /// <param name="id">The ID of the ticket.</param>
        /// <returns>The support ticket data.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            try
            {
                var ticket = await _supportTicketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("ไม่พบ Ticket ที่ระบุ"));
                }
                return Ok(ApiResponse<object>.SuccessResponse(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket with ID {TicketId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาดในการดึงข้อมูล Ticket: {ex.GetBaseException().Message}"));
            }
        }

        /// <summary>
        /// Gets all tickets for the currently authenticated user.
        /// </summary>
        [HttpGet("mytickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var tickets = await _supportTicketService.GetMyTicketsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(tickets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user's tickets.");
                return StatusCode(500, ApiResponse.ErrorResponse("เกิดข้อผิดพลาดในการดึงข้อมูล Ticket ของคุณ"));
            }
        }
    }
}
