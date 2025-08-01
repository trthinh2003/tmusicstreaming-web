using TMusicStreaming.DTOs.History;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IHistoryService
    {
        Task<PagedResult<HistoryDTO>> GetUserHistoryAsync(int userId, int page = 1, int pageSize = 20);
        Task<List<HistoryDTO>> GetRecentHistoryAsync(int userId, int limit = 10);
        Task<bool> CreateHistoryAsync(int userId, CreateHistoryDTO dto);
        Task<bool> CreateBulkHistoryAsync(int userId, BulkCreateHistoryDTO dto);
        Task<bool> DeleteHistoryAsync(int userId, int historyId);
        Task<Dictionary<string, object>> GetUserListeningStatsAsync(int userId);
    }
}
