using TMusicStreaming.DTOs.Song;

namespace TMusicStreaming.DTOs.Album
{
    public class AlbumWithSongsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string ImageUrl { get; set; }
        public string RealeaseDate { get; set; }
        public List<SongDTO> Songs { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
