namespace TMusicStreaming.Models
{
    public class UserInteraction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
        public int PlayCount { get; set; } = 0;
        public bool IsLiked { get; set; } = false;
        public bool IsAddedToPlaylist { get; set; } = false;
        public bool IsDownloaded { get; set; } = false;

        // Điểm số tổng hợp để sử dụng cho gợi ý
        public double InteractionScore { get; set; } = 0;

        public DateTime LastInteractedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}