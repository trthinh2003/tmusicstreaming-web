using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Favorite
{
    public class RemoveFavoriteDTO
    {
        [Required]
        public int SongId { get; set; }
    }
}
