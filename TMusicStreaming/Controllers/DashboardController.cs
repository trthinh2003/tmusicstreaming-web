using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.Services.Interfaces;
using TMusicStreaming.DTOs.Dashboard;
using TMusicStreaming.DTOs.Common; // For DateRangeFilterDTO

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;
        private readonly ICloudinaryService _cloudinaryService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService,
            ICloudinaryService cloudinaryService
        )
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var summary = await _dashboardService.GetOverallSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, new { error = "Error retrieving dashboard summary", details = ex.Message });
            }
        }

        [HttpGet("songs/top-plays")]
        public async Task<IActionResult> GetTopSongsByPlays([FromQuery] int limit = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var filter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
                var songs = await _dashboardService.GetTopSongsByPlaysAsync(limit, filter);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top songs by plays");
                return StatusCode(500, new { error = "Error retrieving top songs by plays", details = ex.Message });
            }
        }

        [HttpGet("songs/top-downloads")]
        public async Task<IActionResult> GetTopSongsByDownloads([FromQuery] int limit = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var filter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
                var songs = await _dashboardService.GetTopSongsByDownloadsAsync(limit, filter);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top songs by downloads");
                return StatusCode(500, new { error = "Error retrieving top songs by downloads", details = ex.Message });
            }
        }

        [HttpGet("songs/top-favorites")]
        public async Task<IActionResult> GetTopSongsByFavorites([FromQuery] int limit = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var filter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
                var songs = await _dashboardService.GetTopSongsByFavoritesAsync(limit, filter);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top songs by favorites");
                return StatusCode(500, new { error = "Error retrieving top songs by favorites", details = ex.Message });
            }
        }

        [HttpGet("songs/genre-distribution")]
        public async Task<IActionResult> GetSongCountByGenre([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var filter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
                var genres = await _dashboardService.GetSongCountByGenreAsync(filter);
                return Ok(genres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting song count by genre");
                return StatusCode(500, new { error = "Error retrieving song count by genre", details = ex.Message });
            }
        }

        [HttpGet("trends/new-users")]
        public async Task<IActionResult> GetNewUsersTrend([FromQuery] string period = "monthly", [FromQuery] int count = 12)
        {
            try
            {
                var trend = await _dashboardService.GetNewUsersTrendAsync(period, count);
                return Ok(trend);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new users trend");
                return StatusCode(500, new { error = "Error retrieving new users trend", details = ex.Message });
            }
        }

        [HttpGet("trends/new-songs")]
        public async Task<IActionResult> GetNewSongsTrend([FromQuery] string period = "monthly", [FromQuery] int count = 12)
        {
            try
            {
                var trend = await _dashboardService.GetNewSongsTrendAsync(period, count);
                return Ok(trend);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new songs trend");
                return StatusCode(500, new { error = "Error retrieving new songs trend", details = ex.Message });
            }
        }

        [HttpGet("trends/total-plays")]
        public async Task<IActionResult> GetTotalPlaysTrend([FromQuery] string period = "monthly", [FromQuery] int count = 12)
        {
            try
            {
                var trend = await _dashboardService.GetTotalPlaysTrendAsync(period, count);
                return Ok(trend);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total plays trend");
                return StatusCode(500, new { error = "Error retrieving total plays trend", details = ex.Message });
            }
        }


        [HttpGet("cloudinary-usage")]
        public async Task<IActionResult> GetCloudinaryUsage()
        {
            try
            {
                var usageResult = await _cloudinaryService.GetUsageAsync();

                var bandwidthUsedMB = usageResult.Bandwidth.Used / 1024.0 / 1024.0;
                var bandwidthLimitMB = usageResult.Bandwidth.Limit / 1024.0 / 1024.0;

                double percentUsed = 0;
                if (usageResult.Bandwidth.Limit > 0)
                {
                    percentUsed = usageResult.Bandwidth.Used * 100.0 / usageResult.Bandwidth.Limit;
                }

                return Ok(new
                {
                    bandwidth = bandwidthUsedMB,
                    limit = bandwidthLimitMB,
                    percentUsed = percentUsed,
                    lastUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Cloudinary usage");
                return StatusCode(500, new
                {
                    error = "Error retrieving Cloudinary usage data",
                    details = ex.Message
                });
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] DashboardFilterDTO filter)
        {
            try
            {
                var pdfBytes = await _dashboardService.GeneratePdfReportAsync(filter);
                return File(pdfBytes, "application/pdf", $"DashboardReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                return StatusCode(500, new { error = "Error generating PDF report", details = ex.Message });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] DashboardFilterDTO filter)
        {
            try
            {
                var excelBytes = await _dashboardService.GenerateExcelReportAsync(filter);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DashboardReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                return StatusCode(500, new { error = "Error generating Excel report", details = ex.Message });
            }
        }
    }
}