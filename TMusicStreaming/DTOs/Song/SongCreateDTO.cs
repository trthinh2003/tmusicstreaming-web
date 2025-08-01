using System.ComponentModel.DataAnnotations;
using TMusicStreaming.Helpers;

namespace TMusicStreaming.DTOs.Song
{
    public class SongCreateDTO
    {
        [Required(ErrorMessage = "Tên bài hát là bắt buộc.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nghệ sĩ là bắt buộc.")]
        public string Artist { get; set; } = string.Empty;

        [JsonArrayNotEmptyAttributeHelper(ErrorMessage = "Vui lòng chọn một thể loại.")]
        public string Genres { get; set; } = string.Empty;// JSON string: "[2,5]"

        [Required(ErrorMessage = "Vui lòng nhập độ dài bài hát.")]
        public string Duration { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày phát hành bài hát.")]
        public DateTime? ReleaseDate { get; set; } = DateTime.Now;

        public string? Album { get; set; } = null;
        public string? cour { get; set; } = string.Empty;

        [JsonArrayNotEmptyAttributeHelper(ErrorMessage = "Vui lòng nhập ít nhất một tag.")]
        public string Tags { get; set; } = string.Empty; // JSON string: ["eqê","qeqe"]

        [Required(ErrorMessage = "Vui lòng chọn ảnh nền.")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh bài hát.")]
        public IFormFile Cover { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn file nhạc.")]
        public IFormFile SongFile { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file lyric.")]
        public IFormFile? LyricsFile { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
