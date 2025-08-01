using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("for-you")]
        public async Task<IActionResult> GetRecommendationsForUser([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId, limit);
                return Ok(new { success = true, data = recommendations });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("artists")]
        public async Task<IActionResult> GetRecommendedArtists([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var artists = await _recommendationService.GetRecommendedArtistsAsync(userId, limit);
                return Ok(new { success = true, data = artists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("playlists")]
        public async Task<IActionResult> GetRecommendedPlaylists([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var playlists = await _recommendationService.GetRecommendedPlaylistsAsync(userId, limit);
                return Ok(new { success = true, data = playlists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("similar/{songId}")]
        public async Task<IActionResult> GetSimilarSongs(int songId, [FromQuery] int limit = 10)
        {
            try
            {
                var similarSongs = await _recommendationService.GetSimilarSongsAsync(songId, limit);
                return Ok(new { success = true, data = similarSongs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSongs([FromQuery] int limit = 10)
        {
            try
            {
                var popularSongs = await _recommendationService.GetPopularSongsAsync(limit);
                return Ok(new { success = true, data = popularSongs });
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
