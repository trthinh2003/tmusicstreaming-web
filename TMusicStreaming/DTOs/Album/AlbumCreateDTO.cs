namespace TMusicStreaming.DTOs.Album
{
    public class AlbumCreateDTO
    {
        public string Title { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int ArtistId { get; set; }
    }
}
