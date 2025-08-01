using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.DTOs.Playlist;
using TMusicStreaming.Helpers;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IUserInteractionService _userInteractionService;
        private readonly TMusicStreamingContext _context;

        public PlaylistsController(IPlaylistRepository playlistRepository, TMusicStreamingContext context, IUserInteractionService userInteractionService)
        {
            _playlistRepository = playlistRepository;
            _context = context;
            _userInteractionService = userInteractionService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPlaylists()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);

            return Ok(playlists);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("popular")]
        public async Task<IActionResult> GetPlaylistsPopular([FromQuery] int page = 1, [FromQuery] int pageSize = 6, string query = "")
        {
            try
            {
                var playlists = await _playlistRepository.GetPlaylistsPopularAsync(page, pageSize, query);
                var totalItems = await _playlistRepository.GetPlaylistsPopularCountAsync(query);
                var response = new PagedResponse<PlaylistPopularDTO>
                {
                    Data = playlists,
                    Pagination = new PaginationInfo
                    {
                        TotalItems = totalItems,
                        CurrentPage = page,
                        PerPage = pageSize,
                        LastPage = (int)Math.Ceiling((double)totalItems / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server khi lấy danh sách playlist", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetPlaylistsByUser(int userId)
        {
            var playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);
            return Ok(playlists);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetPlaylist(int id)
        {
            var playlist = await _playlistRepository.GetPlaylistWithSongsAsync(id);
            if (playlist == null)
                return NotFound();
            return Ok(playlist);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetSongsInPlaylist(int id)
        {
            var playlist = await _playlistRepository.GetPlaylistWithSongsAsync(id);

            if (playlist == null)
                return NotFound(new { message = "Playlist không tồn tại." });

            return Ok(playlist.Songs);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] PlaylistCreateDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var playlist = new Playlist
            {
                Name = dto.Name,
                Description = dto.Description,
                Image = dto.Image,
                UserId = userId
            };

            await _playlistRepository.AddAsync(playlist);
            await _playlistRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, dto);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadPlaylistImage([FromForm] UploadPlaylistImageDTO dto)
        {
            var file = dto.File;
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file được tải lên." });

            try
            {
                var cloudinaryService = HttpContext.RequestServices.GetRequiredService<ICloudinaryService>();
                var imageUrl = await cloudinaryService.UploadFileAsync(file, "TMusicStreaming/playlists/images");

                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { message = "Tải ảnh lên thất bại." });

                return Ok(new { url = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server khi tải ảnh lên.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePlaylist(int id, [FromBody] PlaylistUpdateDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var existingPlaylist = await _context.Playlists.FindAsync(id);
            if (existingPlaylist == null) return NotFound();
            if (existingPlaylist.UserId != userId) return Forbid();

            var playlist = new Playlist
            {
                Name = dto.Name,
                Description = dto.Description,
                Image = dto.Image,
                UserId = userId
            };

            await _playlistRepository.UpdateAsync(id, playlist);
            await _playlistRepository.SaveChangesAsync();

            return Ok(playlist);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPatch("{id}/privacy")]
        public async Task<IActionResult> UpdatePrivacyPlaylist(int id, [FromBody] UpdatePrivacyDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            // Kiểm tra quyền sở hữu
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null) return NotFound();
            if (playlist.UserId != int.Parse(userIdClaim)) return Forbid();

            await _playlistRepository.UpdatePrivacyAsync(id, dto.IsDisplay);
            await _playlistRepository.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("{id}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(int id, [FromBody] AddSongToPlaylistDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int userId = int.Parse(userIdClaim);

                var songId = dto.SongId;
                var result = await _playlistRepository.AddSongToPlaylistAsync(id, songId);
                if (!result)
                    return BadRequest(new { message = "Không thể thêm bài hát vào playlist." });

                // Ghi nhận tương tác thêm vào playlist -> +3 điểm
                await _userInteractionService.RecordPlaylistInteractionAsync(userId, songId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}/songs")]
        public async Task<IActionResult> RemoveSongFromPlaylist(int id, [FromQuery] int songId)
        {
            var result = await _playlistRepository.RemoveSongFromPlaylistAsync(id, songId);
            if (!result)
                return BadRequest(new { message = "Không thể xóa bài hát khỏi playlist." });

            return Ok(new { message = "Xóa bài hát khỏi playlist thành công." });
        }

        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _playlistRepository.DeleteAsync(id);
            await _playlistRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}
