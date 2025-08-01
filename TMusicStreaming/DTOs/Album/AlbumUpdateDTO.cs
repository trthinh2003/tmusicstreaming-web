using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Album
{
    public class AlbumUpdateDTO
    {
        [Required(ErrorMessage = "Tiêu đề album là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn nghệ sĩ")]
        public int ArtistId { get; set; }

        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        public IFormFile? Image { get; set; }
    }
}
