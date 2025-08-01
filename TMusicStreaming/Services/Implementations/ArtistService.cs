using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;

        public ArtistService(IArtistRepository artistRepository)
        {
            _artistRepository = artistRepository;
        }

        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId)
        {
            return await _artistRepository.GetArtistsWithSongsAsync(userId);
        }
        public async Task<ArtistWithSongsDTO?> GetArtistWithSongsAsync(int artistId, int userId)
        {
            return await _artistRepository.GetArtistWithSongsAsync(artistId, userId);
        }

        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize)
        {
            return await _artistRepository.GetArtistsWithSongsAsync(userId, pageNumber, pageSize);
        }
        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize, string query = "")
        {
            return await _artistRepository.GetArtistsWithSongsAsync(userId, pageNumber, pageSize, query);
        }

        public async Task<int> GetTotalArtistsCountAsync(int userId)
        {
            return await _artistRepository.GetTotalArtistsCountAsync(userId);
        }
        public async Task<int> GetTotalArtistsCountAsync(int userId, string query = "")
        {
            return await _artistRepository.GetTotalArtistsCountAsync(userId, query);
        }

        public async Task<bool> FollowArtistAsync(int userId, int artistId)
        {
            return await _artistRepository.FollowArtistAsync(userId, artistId);
        }

        public async Task<bool> UnfollowArtistAsync(int userId, int artistId)
        {
            return await _artistRepository.UnfollowArtistAsync(userId, artistId);
        }

        public async Task<List<ArtistWithSongsDTO>> GetFollowedArtistsAsync(int userId, int pageNumber, int pageSize)
        {
            return await _artistRepository.GetFollowedArtistsAsync(userId, pageNumber, pageSize);
        }

        public async Task<int> GetFollowedArtistsCountAsync(int userId)
        {
            return await _artistRepository.GetFollowedArtistsCountAsync(userId);
        }

        public async Task<bool> IsFollowingArtistAsync(int userId, int artistId)
        {
            return await _artistRepository.IsFollowingArtistAsync(userId, artistId);
        }
    }
}
