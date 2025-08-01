namespace TMusicStreaming.DTOs.Comment
{
    public class CommentReplyDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserBasicDTO User { get; set; } = new();
    }
}