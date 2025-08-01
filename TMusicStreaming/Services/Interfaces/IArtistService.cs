using TMusicStreaming.DTOs.Artist;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId);
        Task<ArtistWithSongsDTO?> GetArtistWithSongsAsync(int artistId, int userId);
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize);
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize, string query = "");
        Task<int> GetTotalArtistsCountAsync(int userId);
        Task<int> GetTotalArtistsCountAsync(int userId, string query = "");
        Task<bool> FollowArtistAsync(int userId, int artistId);
        Task<bool> UnfollowArtistAsync(int userId, int artistId);
        Task<List<ArtistWithSongsDTO>> GetFollowedArtistsAsync(int userId, int pageNumber, int pageSize);
        Task<int> GetFollowedArtistsCountAsync(int userId);
        Task<bool> IsFollowingArtistAsync(int userId, int artistId);
    }
}
