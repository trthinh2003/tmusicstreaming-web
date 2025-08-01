using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.DTOs.Download;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;
        private readonly IUserInteractionService _userInteractionService;

        public DownloadController(IDownloadService downloadService, IUserInteractionService userInteractionService)
        {
            _downloadService = downloadService;
            _userInteractionService = userInteractionService;
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordDownload([FromBody] RecordDownloadRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var download = await _downloadService.RecordDownloadAsync(userId, request.SongId);

                // Ghi nhận tương tác download -> +4 điểm
                await _userInteractionService.RecordDownloadInteractionAsync(userId, request.SongId);

                return Ok(new { success = true, data = download });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("my-downloads")]
        public async Task<IActionResult> GetMyDownloads([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var downloads = await _downloadService.GetUserDownloadsAsync(userId, page, pageSize);

                return Ok(new { success = true, data = downloads });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("check/{songId}")]
        public async Task<IActionResult> CheckDownloadStatus(int songId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var hasDownloaded = await _downloadService.HasUserDownloadedSongAsync(userId, songId);

                return Ok(new { success = true, hasDownloaded });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("stats/{songId}")]
        public async Task<IActionResult> GetDownloadStats(int songId)
        {
            try
            {
                var stats = await _downloadService.GetDownloadStatsAsync(songId);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
