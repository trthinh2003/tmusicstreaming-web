namespace TMusicStreaming.Models
{
    public class CommentReply
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
        public DateTime ReplyCreatedAt { get; set; } = DateTime.UtcNow;

        public Comment? Comment { get; set; }
        public User? User { get; set; }
    }
}
