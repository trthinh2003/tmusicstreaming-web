using AutoMapper;
using TMusicStreaming.DTOs.Comment;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        #region Client
        public async Task<List<CommentDTO>> GetCommentsBySongIdAsync(int songId, int? currentUserId = null)
        {
            var comments = await _commentRepository.GetCommentsBySongIdAsync(songId);
            var commentDTOs = new List<CommentDTO>();

            foreach (var comment in comments)
            {
                var likeCount = await _commentRepository.GetCommentLikeCountAsync(comment.Id);
                var isLiked = currentUserId.HasValue
                    ? await _commentRepository.GetCommentLikeAsync(comment.Id, currentUserId.Value) != null
                    : false;

                var replies = await _commentRepository.GetCommentRepliesAsync(comment.Id);
                var replyDTOs = replies.Select(MapToCommentReplyDTO).ToList();

                var commentDTO = MapToCommentDTO(comment, likeCount, isLiked, replyDTOs);
                commentDTOs.Add(commentDTO);
            }

            return commentDTOs;
        }

        public async Task<CommentDTO> CreateCommentAsync(CreateCommentDTO createCommentDTO, int userId)
        {
            var comment = new Comment
            {
                Content = createCommentDTO.Content.Trim(),
                SongId = createCommentDTO.SongId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.CreateCommentAsync(comment);
            return MapToCommentDTO(createdComment, 0, false, new List<CommentReplyDTO>());
        }

        public async Task<CommentReplyDTO> CreateCommentReplyAsync(CreateCommentReplyDTO createReplyDTO, int userId)
        {
            var reply = new CommentReply
            {
                ReplyContent = createReplyDTO.ReplyContent.Trim(),
                CommentId = createReplyDTO.CommentId,
                UserId = userId,
                ReplyCreatedAt = DateTime.UtcNow
            };

            var createdReply = await _commentRepository.CreateCommentReplyAsync(reply);
            return MapToCommentReplyDTO(createdReply);
        }

        public async Task<bool> ToggleCommentLikeAsync(int commentId, int userId)
        {
            var existingLike = await _commentRepository.GetCommentLikeAsync(commentId, userId);

            if (existingLike != null)
            {
                await _commentRepository.RemoveCommentLikeAsync(existingLike);
                return false; // Unlike
            }
            else
            {
                var newLike = new CommentLike
                {
                    CommentId = commentId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                await _commentRepository.AddCommentLikeAsync(newLike);
                return true; // Like
            }
        }
        #endregion

        #region Admin
        public async Task<PagedResponse<CommentAdminDTO>> GetCommentsForAdminAsync(int page, int pageSize)
        {
            var comments = await _commentRepository.GetCommentsWithPaginationAsync(page, pageSize);
            var totalCount = await _commentRepository.GetTotalCommentsCountAsync();

            var commentAdminDTOs = new List<CommentAdminDTO>();
            foreach (var comment in comments)
            {
                var likeCount = await _commentRepository.GetCommentLikeCountAsync(comment.Id);
                var replies = await _commentRepository.GetCommentRepliesAsync(comment.Id);

                commentAdminDTOs.Add(new CommentAdminDTO
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt ?? DateTime.UtcNow,
                    UserId = comment.UserId,
                    SongTitle = comment.Song?.Title ?? "Unknown",
                    SongId = comment.SongId,
                    LikeCount = likeCount,
                    ReplyCount = replies.Count
                });
            }

            return new PagedResponse<CommentAdminDTO>
            {
                Data = commentAdminDTOs,
                Pagination = new PaginationInfo
                {
                    TotalItems = totalCount,
                    CurrentPage = page,
                    PerPage = pageSize,
                    LastPage = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }

        public async Task<PagedResponse<CommentAdminDTO>> SearchCommentsForAdminAsync(string searchTerm, int page, int pageSize)
        {
            var comments = await _commentRepository.SearchCommentsAsync(searchTerm, page, pageSize);
            var totalCount = await _commentRepository.GetSearchCommentsCountAsync(searchTerm);

            var commentAdminDTOs = new List<CommentAdminDTO>();
            foreach (var comment in comments)
            {
                var likeCount = await _commentRepository.GetCommentLikeCountAsync(comment.Id);
                var replies = await _commentRepository.GetCommentRepliesAsync(comment.Id);

                commentAdminDTOs.Add(new CommentAdminDTO
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt ?? DateTime.UtcNow,
                    UserId = comment.UserId,
                    SongTitle = comment.Song?.Title ?? "Unknown",
                    SongId = comment.SongId,
                    LikeCount = likeCount,
                    ReplyCount = replies.Count
                });
            }

            return new PagedResponse<CommentAdminDTO>
            {
                Data = commentAdminDTOs,
                Pagination = new PaginationInfo
                {
                    TotalItems = totalCount,
                    CurrentPage = page,
                    PerPage = pageSize,
                    LastPage = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }
        #endregion

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _commentRepository.DeleteCommentAsync(commentId);
        }

        // Mapping methods
        private CommentDTO MapToCommentDTO(Comment comment, int likeCount, bool isLiked, List<CommentReplyDTO> replies)
        {
            return new CommentDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt ?? DateTime.UtcNow,
                User = MapToUserBasicDTO(comment.User),
                LikeCount = likeCount,
                IsLiked = isLiked,
                Replies = replies
            };
        }

        private CommentReplyDTO MapToCommentReplyDTO(CommentReply reply)
        {
            return new CommentReplyDTO
            {
                Id = reply.Id,
                Content = reply.ReplyContent,
                CreatedAt = reply.ReplyCreatedAt,
                User = MapToUserBasicDTO(reply.User)
            };
        }

        private UserBasicDTO MapToUserBasicDTO(User? user)
        {
            if (user == null) return new UserBasicDTO();

            return new UserBasicDTO
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar
            };
        }
    }
}
