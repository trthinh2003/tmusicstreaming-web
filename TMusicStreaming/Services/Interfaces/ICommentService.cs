using TMusicStreaming.DTOs.Comment;
using TMusicStreaming.DTOs.Paginate;

namespace TMusicStreaming.Services.Interfaces
{
    public interface ICommentService
    {
        //Client
        Task<List<CommentDTO>> GetCommentsBySongIdAsync(int songId, int? currentUserId = null);
        Task<CommentDTO> CreateCommentAsync(CreateCommentDTO createCommentDTO, int userId);
        Task<CommentReplyDTO> CreateCommentReplyAsync(CreateCommentReplyDTO createReplyDTO, int userId);
        Task<bool> ToggleCommentLikeAsync(int commentId, int userId);

        //Admin
        Task<PagedResponse<CommentAdminDTO>> GetCommentsForAdminAsync(int page, int pageSize);
        Task<PagedResponse<CommentAdminDTO>> SearchCommentsForAdminAsync(string searchTerm, int page, int pageSize);
        Task<bool> DeleteCommentAsync(int commentId);
    }
}
