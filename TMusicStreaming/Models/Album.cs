using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.Models
{
    public class Album
    {
        public int Id { get; set; }

        [MaxLength(255)] 
        public string Title { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }
        public List<Song> Songs { get; set; } = new List<Song>();
        public Artist Artist { get; set; }
        public int ArtistId { get; set; }
        public bool isDisplay { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
