namespace TMusicStreaming.DTOs.Genre
{
    public class UpdateGenreDTO
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
