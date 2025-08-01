using System.ComponentModel.DataAnnotations.Schema;

namespace TMusicStreaming.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public Song Song { get; set; }
        public int SongId { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<CommentReply> CommentReplies { get; set; } = new();
        public List<CommentLike> CommentLikes { get; set; } = new();
    }
}
