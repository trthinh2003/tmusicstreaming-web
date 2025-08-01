namespace TMusicStreaming.Models
{
    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; } 
        public int UserId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        public Comment? Comment { get; set; }
        public User? User { get; set; }
    }
}
