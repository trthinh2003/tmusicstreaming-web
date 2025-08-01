using TMusicStreaming.DTOs.Dashboard;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> CountAlbumsAsync();
        Task<int> CountArtistsAsync();
        Task<int> CountSongsAsync();
        Task<int> CountUsersAsync();
        Task<int> CountDownloadsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> CountHistoriesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<SongStatisticDTO>> GetTopSongsByPlaysAsync(int limit, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<SongStatisticDTO>> GetTopSongsByDownloadsAsync(int limit, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<SongStatisticDTO>> GetTopSongsByFavoritesAsync(int limit, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetSongCountByGenreAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetNewUsersCountByPeriodAsync(string period, int count); // period: "daily", "weekly", "monthly"
        Task<Dictionary<string, int>> GetNewSongsCountByPeriodAsync(string period, int count);
        Task<Dictionary<string, int>> GetTotalPlaysByPeriodAsync(string period, int count);
    }
}