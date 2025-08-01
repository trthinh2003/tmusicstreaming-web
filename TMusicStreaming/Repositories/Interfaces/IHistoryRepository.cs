using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IHistoryRepository
    {
        Task<List<History>> GetUserHistoryAsync(int userId, int page = 1, int pageSize = 20);
        Task<History?> GetHistoryByIdAsync(int historyId);
        Task<bool> CreateHistoryAsync(History history);
        Task<bool> CreateBulkHistoryAsync(List<History> histories);
        Task<bool> DeleteHistoryAsync(int historyId);
        Task<List<History>> GetRecentHistoryAsync(int userId, int limit = 10);
        Task<bool> HasUserListenedSongAsync(int userId, int songId, DateTime since);
        Task<int> GetTotalUserHistoryCountAsync(int userId);
    }
}
