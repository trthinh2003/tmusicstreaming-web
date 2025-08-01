using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Avatar { get; set; } = string.Empty;

        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)] 
        public string PasswordHash { get; set; } = string.Empty;

        public bool Gender { get; set; }

        public string Role { get; set; } = "User";

        public string PlatformId { get; set; } = string.Empty;

        public string PlatformName { get; set; } = string.Empty;

        public int Status { get; set; } = 0;

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
        public List<History> Histories { get; set; } = new List<History>();
        public ICollection<Follow> Follows { get; set; } = new List<Follow>();
    }
}
