namespace TMusicStreaming.Models
{
    public class UserSimilarity
    {
        public int Id { get; set; }
        public int UserId1 { get; set; }
        public User User1 { get; set; }
        public int UserId2 { get; set; }
        public User User2 { get; set; }
        public double SimilarityScore { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}