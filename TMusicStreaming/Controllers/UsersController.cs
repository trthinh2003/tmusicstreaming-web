using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.User;
using TMusicStreaming.Helpers;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository repo, IUserService userService, ILogger<UsersController> logger)
        {
            _userRepo = repo;
            _userService = userService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var users = await _userRepo.GetAllUserAsync();
                var pagedResponse = PaginationHelper.CreatePagedResponse(users, page, pageSize);
                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách người dùng.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return await GetAllUsers(page, pageSize);
                }

                var users = await _userRepo.SearchUsersAsync(query.Trim());
                var pagedResponse = PaginationHelper.CreatePagedResponse(users, page, pageSize);
                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", query);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm kiếm người dùng.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                if (id == 1)
                {
                    return BadRequest(new { message = "Không thể xóa tài khoản quản trị viên." });
                }

                var currentUserId = GetCurrentUserId();
                if (id == currentUserId)
                {
                    return BadRequest(new { message = "Không thể xóa chính tài khoản của bạn." });
                }

                var result = await _userRepo.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng cần xóa." });
                }

                return Ok(new { message = "Xóa người dùng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa người dùng.", error = ex.Message });
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userProfile = await _userService.GetUserProfileAsync(userId);

                if (userProfile == null)
                {
                    return NotFound(new { message = "Không tìm thấy hồ sơ người dùng." });
                }

                return Ok(userProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to GetProfile.");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin hồ sơ người dùng.");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin hồ sơ.", error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request, IFormFile? avatarFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedProfile = await _userService.UpdateUserProfileAsync(userId, request, avatarFile);

                if (updatedProfile == null)
                {
                    return BadRequest(new { message = "Cập nhật hồ sơ thất bại." });
                }

                return Ok(new { message = "Cập nhật hồ sơ thành công!", user = updatedProfile });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to UpdateProfile.");
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during UpdateProfile: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin hồ sơ người dùng.");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật hồ sơ.", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc ID người dùng không hợp lệ.");
            }
            return userId;
        }
    }
}