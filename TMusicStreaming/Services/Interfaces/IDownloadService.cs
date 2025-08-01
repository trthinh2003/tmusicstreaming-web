using TMusicStreaming.DTOs.Download;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<DownloadDTO> RecordDownloadAsync(int userId, int songId);
        Task<List<DownloadDTO>> GetUserDownloadsAsync(int userId, int page = 1, int pageSize = 20);
        Task<bool> HasUserDownloadedSongAsync(int userId, int songId);
        Task<DownloadStatsDTO> GetDownloadStatsAsync(int songId);
    }
}
