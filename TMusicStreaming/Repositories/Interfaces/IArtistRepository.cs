using TMusicStreaming.DTOs.Artist;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IArtistRepository
    {
        Task<List<ArtistDTO>> GetAllArtistAsync();
        Task<ArtistDTO> GetArtistAsync(int id);
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId);
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize);
        Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize, string query = "");
        Task<ArtistWithSongsDTO?> GetArtistWithSongsAsync(int artistId, int userId);
        Task<int> GetTotalArtistsCountAsync(int userId);
        Task<int> GetTotalArtistsCountAsync(int userId, string query = "");
        Task<bool> FollowArtistAsync(int userId, int artistId);
        Task<bool> UnfollowArtistAsync(int userId, int artistId);
        Task<List<ArtistWithSongsDTO>> GetFollowedArtistsAsync(int userId, int pageNumber, int pageSize);
        Task<int> GetFollowedArtistsCountAsync(int userId);
        Task<bool> IsFollowingArtistAsync(int userId, int artistId);
        Task<List<ArtistDTO>> SearchArtistAsync(string query);
        Task<bool> CreateArtistAsync(ArtistCreateDTO artist, string avatarUrl);
        Task<bool> UpdateArtistAsync(int id, ArtistCreateDTO artist, string avatarUrl);
        Task DeleteArtistAsync(int id);

        Task<int> CountArtistsAsync();
    }
}
