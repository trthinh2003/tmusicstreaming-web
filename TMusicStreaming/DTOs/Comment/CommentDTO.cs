namespace TMusicStreaming.DTOs.Comment
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserBasicDTO User { get; set; } = new();
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public List<CommentReplyDTO> Replies { get; set; } = new();
    }
}
