namespace TMusicStreaming.DTOs.Song
{
    public class SongForArtistDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Artist { get; set; } = string.Empty;
        public string? Cover { get; set; } = string.Empty;
        public string? Audio { get; set; } = string.Empty;
        public string? Duration { get; set; } = string.Empty;
        public string? Lyric { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string? Tags { get; set; } = string.Empty;
        public int? AlbumId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? isDisplay { get; set; } = false;
        public bool? isLossless { get; set; } = false;
        public bool? isPopular { get; set; } = false;
    }
}
