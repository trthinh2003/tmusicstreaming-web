using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Comment
{
    public class CreateCommentReplyDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string ReplyContent { get; set; } = string.Empty;

        [Required]
        public int CommentId { get; set; }
    }
}
