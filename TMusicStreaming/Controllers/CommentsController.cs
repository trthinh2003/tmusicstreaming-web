using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.DTOs.Comment;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{songId}")]
        public async Task<ActionResult<List<CommentDTO>>> GetComments(int songId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var comments = await _commentService.GetCommentsBySongIdAsync(songId, currentUserId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi lấy bình luận: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentDTO>> CreateComment([FromBody] CreateCommentDTO createCommentDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("Vui lòng đăng nhập để bình luận");

                var comment = await _commentService.CreateCommentAsync(createCommentDTO, userId.Value);
                return CreatedAtAction(nameof(GetComments), new { songId = createCommentDTO.SongId }, comment);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tạo bình luận: {ex.Message}");
            }
        }

        [HttpPost("reply")]
        [Authorize]
        public async Task<ActionResult<CommentReplyDTO>> CreateReply([FromBody] CreateCommentReplyDTO createReplyDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("Vui lòng đăng nhập để trả lời bình luận");

                var reply = await _commentService.CreateCommentReplyAsync(createReplyDTO, userId.Value);
                return Ok(reply);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tạo trả lời: {ex.Message}");
            }
        }

        [HttpPost("{commentId}/like")]
        [Authorize]
        public async Task<ActionResult<object>> ToggleLike(int commentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("Vui lòng đăng nhập để thích bình luận");

                var isLiked = await _commentService.ToggleCommentLikeAsync(commentId, userId.Value);
                return Ok(new { isLiked });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi thích/bỏ thích bình luận: {ex.Message}");
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
