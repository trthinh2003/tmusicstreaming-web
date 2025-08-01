namespace TMusicStreaming.DTOs.Comment
{
    public class CommentAdminDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string SongTitle { get; set; } = string.Empty;
        public int SongId { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
    }
}
