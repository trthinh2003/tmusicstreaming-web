using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Artist
{
    public class ArtistDTO
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2048)]
        public string Avatar { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
