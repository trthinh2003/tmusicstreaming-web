using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Favorite
{
    public class AddFavoriteDTO
    {
        [Required]
        public int SongId { get; set; }
    }
}
