using TMusicStreaming.DTOs.Song;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface ISongRepository
    {
        Task<List<SongDTO>> GetAllSongsAsync(int? userId = null);
        Task<List<SongDTO>> GetRandomSongsReservoirAsync(int count, int? userId = null);
        Task<List<SongDTO>> SearchSongAsync(string query, int? userId = null);
        Task<List<SongDTO>> GetSongsByPlaylistIdAsync(int playlistId, int? userId = null);
        Task<List<SongDTO>> GetFavoriteSongsByUserIdAsync(int userId);

        Task<List<SongDTO>> GetSongsByArtistAsync(string artistName, int count, int? userId = null);
        Task<SongDTO?> GetSongByIdAsync(int id, int? userId = null);
        Task<SongDTO?> GetSongBySlugAsync(string slug, int? userId = null);
        Task<List<SongDTO>> GetPopularSongsAsync(int count = 5, int? userId = null);
        Task<List<SongDTO>> GetNewReleasesAsync(int count = 5, int? userId = null);
        Task<bool> CreateSongAsync(SongCreateDTO song, List<int> genreIds, List<string> tags, 
            string backgroundUrl, string coverUrl, string songUrl, string lyricUrl);
        Task<bool> UpdateSongAsync(int id, SongCreateDTO song, List<int> genreIds, List<string> tags,
            string backgroundUrl, string coverUrl, string songUrl, string lyricUrl);
        Task<bool> UpdateSongPopularAsync(int songId, bool isPopular);
        Task<bool> DeleteSongAsync(int id);
        
        Task<int> CountSongsAsync();
    }
}
