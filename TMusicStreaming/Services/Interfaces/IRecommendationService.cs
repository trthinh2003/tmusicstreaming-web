using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Playlist;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Models;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<SongDTO>> GetRecommendationsForUserAsync(int userId, int limit = 10);
        Task<List<Song>> GetSimilarSongsAsync(int songId, int limit = 10);
        Task<List<Song>> GetPopularSongsAsync(int limit = 10);
        Task UpdateUserSimilarityAsync(int userId);
        Task<List<ArtistRecommendDTO>> GetRecommendedArtistsAsync(int userId, int limit = 10);
        Task<List<PlaylistRecommendDTO>> GetRecommendedPlaylistsAsync(int userId, int limit = 10);
    }
}
