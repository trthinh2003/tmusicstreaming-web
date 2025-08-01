using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        // Client
        Task<List<Comment>> GetCommentsBySongIdAsync(int songId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<CommentReply> CreateCommentReplyAsync(CommentReply reply);
        Task<CommentLike?> GetCommentLikeAsync(int commentId, int userId);
        Task<CommentLike> AddCommentLikeAsync(CommentLike like);
        Task RemoveCommentLikeAsync(CommentLike like);
        Task<int> GetCommentLikeCountAsync(int commentId);
        Task<List<CommentReply>> GetCommentRepliesAsync(int commentId);

        // Admin
        Task<List<Comment>> GetAllCommentsAsync();
        Task<List<Comment>> GetCommentsWithPaginationAsync(int page, int pageSize);
        Task<int> GetTotalCommentsCountAsync();
        Task<bool> DeleteCommentAsync(int commentId);
        Task<List<Comment>> SearchCommentsAsync(string searchTerm, int page, int pageSize);
        Task<int> GetSearchCommentsCountAsync(string searchTerm);
    }
}
