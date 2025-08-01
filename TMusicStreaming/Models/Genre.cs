namespace TMusicStreaming.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public List<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
    }
}
