using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ITInventoryController(IITInventoryService inventoryService, ILogger<ITInventoryController> logger) : ControllerBase
    {
        /// <summary>
        /// Endpoint to get all available stock items.
        /// </summary>
        [HttpGet("stockitems")]
        public async Task<IActionResult> GetStockItems()
        {
            try
            {
                var items = await inventoryService.GetAvailableStockItemsAsync();
                return Ok(ApiResponse.SuccessResponse(items));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stock items.");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while fetching stock items."));
            }
        }
    }
}
