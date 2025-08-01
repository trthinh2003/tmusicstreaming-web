namespace TMusicStreaming.Models
{
    public class Follow
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
