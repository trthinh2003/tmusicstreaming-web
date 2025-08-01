using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Comment
{
    public class CreateCommentDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int SongId { get; set; }
    }
}
