using System.ComponentModel.DataAnnotations;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Models;

namespace TMusicStreaming.DTOs.Album
{
    public class AlbumDTO
    {
        public int Id { get; set; }

        [MaxLength(255)] 
        public string Title { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        public int SongCount { get; set; }

        public ArtistDTO? Artist { get; set; } = null;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
