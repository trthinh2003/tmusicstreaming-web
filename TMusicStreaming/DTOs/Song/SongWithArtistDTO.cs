namespace TMusicStreaming.DTOs.Song
{
    public class SongWithArtistDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string Genre { get; set; } = "";
        public string Cover { get; set; } = "";
        public string? Audio { get; set;}
        public string? Lyric { get; set;}
        public string? Background { get; set; }
        public string? Duration { get; set; }
    }
}