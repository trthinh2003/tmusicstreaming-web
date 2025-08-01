namespace TMusicStreaming.DTOs.Favorite
{
    public class FavoriteDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public string SongTitle { get; set; } = string.Empty;
        public string SongArtist { get; set; } = string.Empty;
        public string SongImage { get; set; } = string.Empty;
        public string SongSlug { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
