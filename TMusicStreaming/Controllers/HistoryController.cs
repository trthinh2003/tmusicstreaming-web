using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.DTOs.History;
using TMusicStreaming.Models;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger, IUserInteractionService userInteractionService)
        {
            _historyService = historyService;
            _logger = logger;
            _userInteractionService = userInteractionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _historyService.GetUserHistoryAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentHistory([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _historyService.GetRecentHistoryAsync(userId, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHistory([FromBody] CreateHistoryDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _historyService.CreateHistoryAsync(userId, dto);

                if (success)
                {
                    // Ghi nhận tương tác nghe nhạc -> +1 điểm
                    await _userInteractionService.RecordPlayInteractionAsync(userId, dto.SongId);
                    return Ok(new { message = "History created successfully" });
                }

                return BadRequest(new { message = "Failed to create history" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkHistory([FromBody] BulkCreateHistoryDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _historyService.CreateBulkHistoryAsync(userId, dto);

                if (success)
                    return Ok(new { message = "Bulk history created successfully" });

                return BadRequest(new { message = "Failed to create bulk history" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _historyService.DeleteHistoryAsync(userId, id);

                if (success)
                    return Ok(new { message = "History deleted successfully" });

                return NotFound(new { message = "History not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetListeningStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _historyService.GetUserListeningStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting listening stats");
                return StatusCode(500, "Internal server error");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }

}
