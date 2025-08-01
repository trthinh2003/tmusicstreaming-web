using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class AdminCommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public AdminCommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetComments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _commentService.GetCommentsForAdminAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy danh sách bình luận: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchComments([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetComments(page, pageSize);
                }

                var result = await _commentService.SearchCommentsForAdminAsync(searchTerm, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tìm kiếm bình luận: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (result)
                {
                    return Ok(new { message = "Xóa bình luận thành công" });
                }
                return NotFound("Không tìm thấy bình luận");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xóa bình luận: {ex.Message}");
            }
        }
    }
}
