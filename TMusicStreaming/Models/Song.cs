using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMusicStreaming.Models
{
    public class Song
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Artist { get; set; } = string.Empty;

        [MaxLength(255)]
        public string SongFile { get; set; } = string.Empty;

        [MaxLength(255)]
        public string LyricsFile { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public string DurationInSeconds { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }

        public string Cover { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;

        public bool isDisplay { get; set; } = true;
        public bool isLossless { get; set; } = false;
        public bool isPopular { get; set; } = false;

        public int? AlbumId { get; set; }  // nullable nếu bài hát có thể không thuộc album nào
        public Album? Album { get; set; }

        public List<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
