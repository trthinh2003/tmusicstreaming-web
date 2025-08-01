using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.Models
{
    public class Playlist
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public User User { get; set; }
        public int UserId { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool isDisplay { get; set; } = false;
        public List<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
