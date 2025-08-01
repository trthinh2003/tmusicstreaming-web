using TMusicStreaming.DTOs.Song;

namespace TMusicStreaming.DTOs.Artist
{
    public class ArtistWithSongsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsFollowed { get; set; }
        public DateTime? FollowedAt { get; set; }
        public List<SongForArtistDTO> Songs { get; set; } = new List<SongForArtistDTO>();
    }
}
