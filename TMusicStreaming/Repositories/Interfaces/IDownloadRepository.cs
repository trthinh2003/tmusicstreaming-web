using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IDownloadRepository
    {
        Task<Download> CreateDownloadAsync(int userId, int songId);
        Task<List<Download>> GetUserDownloadsAsync(int userId, int page = 1, int pageSize = 20);
        Task<bool> HasDownloadedAsync(int userId, int songId);
        Task<int> GetDownloadCountBySongAsync(int songId);
        Task<List<Download>> GetRecentDownloadsAsync(int userId, int limit = 10);
    }
}
