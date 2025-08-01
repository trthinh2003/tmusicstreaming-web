using TMusicStreaming.DTOs.Dashboard;
using TMusicStreaming.DTOs.Common;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDTO> GetOverallSummaryAsync();
        Task<List<SongStatisticDTO>> GetTopSongsByPlaysAsync(int limit, DateRangeFilterDTO? filter = null);
        Task<List<SongStatisticDTO>> GetTopSongsByDownloadsAsync(int limit, DateRangeFilterDTO? filter = null);
        Task<List<SongStatisticDTO>> GetTopSongsByFavoritesAsync(int limit, DateRangeFilterDTO? filter = null);
        Task<Dictionary<string, int>> GetSongCountByGenreAsync(DateRangeFilterDTO? filter = null);
        Task<Dictionary<string, int>> GetNewUsersTrendAsync(string period = "monthly", int count = 12);
        Task<Dictionary<string, int>> GetNewSongsTrendAsync(string period = "monthly", int count = 12);
        Task<Dictionary<string, int>> GetTotalPlaysTrendAsync(string period = "monthly", int count = 12);

        // Xuất báo cáo
        Task<byte[]> GeneratePdfReportAsync(DashboardFilterDTO filter);
        Task<byte[]> GenerateExcelReportAsync(DashboardFilterDTO filter);
    }
}