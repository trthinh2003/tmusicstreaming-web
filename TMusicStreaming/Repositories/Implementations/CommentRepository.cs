using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly TMusicStreamingContext _context;

        public CommentRepository(TMusicStreamingContext context)
        {
            _context = context;
        }

        #region Client
        public async Task<List<Comment>> GetCommentsBySongIdAsync(int songId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.SongId == songId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return await _context.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);
        }

        public async Task<CommentReply> CreateCommentReplyAsync(CommentReply reply)
        {
            _context.CommentReplies.Add(reply);
            await _context.SaveChangesAsync();

            return await _context.CommentReplies
                .Include(r => r.User)
                .FirstAsync(r => r.Id == reply.Id);
        }

        public async Task<CommentLike?> GetCommentLikeAsync(int commentId, int userId)
        {
            return await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
        }

        public async Task<CommentLike> AddCommentLikeAsync(CommentLike like)
        {
            _context.CommentLikes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task RemoveCommentLikeAsync(CommentLike like)
        {
            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCommentLikeCountAsync(int commentId)
        {
            return await _context.CommentLikes.CountAsync(cl => cl.CommentId == commentId);
        }

        public async Task<List<CommentReply>> GetCommentRepliesAsync(int commentId)
        {
            return await _context.CommentReplies
                .Include(r => r.User)
                .Where(r => r.CommentId == commentId)
                .OrderBy(r => r.ReplyCreatedAt)
                .ToListAsync();
        }
        #endregion

        #region Admin
        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Song)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetCommentsWithPaginationAsync(int page, int pageSize)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Song)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            return await _context.Comments.CountAsync();
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            // Xóa các reply và like liên quan
            var replies = await _context.CommentReplies.Where(r => r.CommentId == commentId).ToListAsync();
            var likes = await _context.CommentLikes.Where(l => l.CommentId == commentId).ToListAsync();

            _context.CommentReplies.RemoveRange(replies);
            _context.CommentLikes.RemoveRange(likes);
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Comment>> SearchCommentsAsync(string searchTerm, int page, int pageSize)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Song)
                .Where(c => c.Content.Contains(searchTerm) ||
                           c.Song.Title.Contains(searchTerm) ||
                           c.User.Name.Contains(searchTerm))
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSearchCommentsCountAsync(string searchTerm)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Song)
                .Where(c => c.Content.Contains(searchTerm) ||
                           c.Song.Title.Contains(searchTerm) ||
                           c.User.Name.Contains(searchTerm))
                .CountAsync();
        }
        #endregion
    }
}
