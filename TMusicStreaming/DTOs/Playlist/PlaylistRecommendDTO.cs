namespace TMusicStreaming.DTOs.Playlist
{
    public class PlaylistRecommendDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Cover => Image;
        public int SongCount { get; set; }
        public string CreatorName { get; set; } = string.Empty;
    }
}
