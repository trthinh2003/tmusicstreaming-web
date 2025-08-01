namespace TMusicStreaming.DTOs.Artist
{
    public class ArtistRecommendDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int Followers { get; set; }
        public bool IsFollowing { get; set; } = false;
    }
}
