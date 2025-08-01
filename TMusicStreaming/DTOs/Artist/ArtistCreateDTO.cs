using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.Artist
{
    public class ArtistCreateDTO
    {
        public string Name { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public IFormFile? Avatar { get; set; } = null!;
    }
}
