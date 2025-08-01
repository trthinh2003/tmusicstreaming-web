using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.DTOs.Favorite;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IUserInteractionService _userInteractionService;

        public FavoriteController(IFavoriteService favoriteService, IUserInteractionService userInteractionService)
        {
            _favoriteService = favoriteService;
            _userInteractionService = userInteractionService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet("check/{songId}")]
        public async Task<IActionResult> CheckFavorite(int songId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, songId);
            return Ok(new { IsFavorite = isFavorite });
        }

        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId, page, pageSize);
            return Ok(favorites);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetFavoriteCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var count = await _favoriteService.GetFavoriteCountAsync(userId);
            return Ok(new { Count = count });
        }

        [HttpGet("song-ids")]
        public async Task<IActionResult> GetFavoriteSongIds()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var songIds = await _favoriteService.GetUserFavoriteSongIdsAsync(userId);
            return Ok(new { SongIds = songIds });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToFavorite([FromBody] AddFavoriteDTO addFavoriteDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var result = await _favoriteService.AddToFavoriteAsync(userId, addFavoriteDTO);

            if (result.IsFavorite)
            {
                // Ghi nhận tương tác yêu thích -> +5 điểm
                await _userInteractionService.RecordLikeInteractionAsync(userId, addFavoriteDTO.SongId, true);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromFavorite([FromBody] RemoveFavoriteDTO removeFavoriteDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var result = await _favoriteService.RemoveFromFavoriteAsync(userId, removeFavoriteDTO);

            if (!result.IsFavorite)
            {
                // Ghi nhận tương tác bỏ yêu thích -> -5 điểm
                await _userInteractionService.RecordLikeInteractionAsync(userId, removeFavoriteDTO.SongId, false);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpPost("toggle/{songId}")]
        public async Task<IActionResult> ToggleFavorite(int songId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Không thể xác định người dùng");

            var result = await _favoriteService.ToggleFavoriteAsync(userId, songId);

            // Ghi nhận tương tác yêu thích/bỏ yêu thích -> +5/-5 điểm tùy vào trạng thái
            await _userInteractionService.RecordLikeInteractionAsync(userId, songId, result.IsFavorite);

            return Ok(result);
        }
    }
}
