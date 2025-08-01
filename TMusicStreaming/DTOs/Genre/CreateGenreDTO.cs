namespace TMusicStreaming.DTOs.Genre
{
    public class CreateGenreDTO
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
