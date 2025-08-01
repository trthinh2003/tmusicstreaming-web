namespace TMusicStreaming.DTOs.Genre
{
    public class GenreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? SongCount { get; set; }
    }
}
